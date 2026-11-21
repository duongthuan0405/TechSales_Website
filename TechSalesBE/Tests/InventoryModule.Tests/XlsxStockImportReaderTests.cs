using System.IO.Compression;
using CommonModule.BusinessExceptions;
using InventoryModule.Application.Constants;
using InventoryModule.Application.Exceptions;
using InventoryModule.Application.Services;
using InventoryModule.Infrastructure.Services;
using Xunit;

namespace InventoryModule.Tests;

public sealed class XlsxStockImportReaderTests
{
    private const string ProductId = "11111111-1111-4111-8111-111111111111";
    private static readonly string Header = DataRow(1, "ProductId", "SerialNumber");
    private readonly XlsxStockImportReader _reader = new();

    [Fact]
    public void PreservesTextSerialsAndUsesThePhysicalExcelRowNumber()
    {
        using Stream workbook = CreateWorkbook(Header + Row(2, "") + DataRow(5, ProductId, "000042"));

        StockImportRow row = Assert.Single(_reader.Read(workbook, CancellationToken.None));

        Assert.Equal(new StockImportRow(5, ProductId, "000042"), row);
        Assert.True(workbook.CanRead);
    }

    [Fact]
    public void AcceptsReversedHeadersAndSharedOrNumericCellValues()
    {
        string rows = DataRow(1, " SerialNumber ", "productid")
            + Row(2, """<c r="A2" t="s"><v>0</v></c>""" + TextCell("B2", ProductId))
            + Row(3, """<c r="A3"><v>42</v></c>""" + TextCell("B3", ProductId));
        using Stream workbook = CreateWorkbook(rows, "<si><r><t>SN-</t></r><r><t>1</t></r></si>");

        List<StockImportRow> data = _reader.Read(workbook, CancellationToken.None);

        Assert.Collection(data,
            row => Assert.Equal(new StockImportRow(2, ProductId, "SN-1"), row),
            row => Assert.Equal(new StockImportRow(3, ProductId, "42"), row));
    }

    [Fact]
    public void RejectsAFileThatIsNotAnXlsxArchive()
    {
        using Stream workbook = new MemoryStream([1, 2, 3, 4]);

        AssertWorkbookError(workbook, "File", BusinessExceptionMessage.InvalidImportWorkbook);
    }

    [Fact]
    public void RejectsInvalidHeaders()
    {
        using Stream workbook = CreateWorkbook(DataRow(1, "ItemId", "SerialNumber") + DataRow(2, ProductId, "SN-1"));

        AssertWorkbookError(workbook, "File", BusinessExceptionMessage.InvalidImportHeaders);
    }

    [Fact]
    public void RejectsAHeaderOnlyWorksheet()
    {
        using Stream workbook = CreateWorkbook(Header);

        AssertWorkbookError(workbook, "File", BusinessExceptionMessage.ImportRowsRequired);
    }

    [Fact]
    public void RejectsAnExtraDataColumnAtTheAffectedRow()
    {
        string cells = TextCell("A2", ProductId) + TextCell("B2", "SN-1") + TextCell("C2", "Extra");
        using Stream workbook = CreateWorkbook(Header + Row(2, cells));

        AssertWorkbookError(workbook, "Rows[2]", BusinessExceptionMessage.UnexpectedImportColumn);
    }

    [Theory]
    [InlineData("""<c r="B2"><f>1+1</f><v>2</v></c>""")]
    [InlineData("""<c r="B2" t="e"><v>#REF!</v></c>""")]
    public void RejectsFormulaAndErrorCells(string cell)
    {
        using Stream workbook = CreateWorkbook(Header + Row(2, TextCell("A2", ProductId) + cell));

        AssertWorkbookError(workbook, "Rows[2]", BusinessExceptionMessage.InvalidImportCell);
    }

    [Fact]
    public void AcceptsTheRowLimitAndRejectsAnAdditionalDataRow()
    {
        string rows = Header + string.Concat(Enumerable.Range(2, StockImportLimits.MaxDataRows)
            .Select(number => DataRow(number, ProductId, $"SN-{number}")));
        using Stream atLimit = CreateWorkbook(rows);
        using Stream overLimit = CreateWorkbook(rows + DataRow(StockImportLimits.MaxDataRows + 2, ProductId, "SN-EXTRA"));

        Assert.Equal(StockImportLimits.MaxDataRows, _reader.Read(atLimit, CancellationToken.None).Count);
        AssertWorkbookError(overLimit, "File", BusinessExceptionMessage.TooManyImportRows);
    }

    private void AssertWorkbookError(Stream workbook, string field, string expectedMessage)
    {
        BusinessException error = Assert.Throws<BusinessException>(() => _reader.Read(workbook, CancellationToken.None));

        Assert.Equal(BusinessExceptionCode.BAD_REQUEST, error.Code);
        Assert.Equal(expectedMessage, Assert.Single(error.Errors[field]));
    }

    private static string TextCell(string reference, string value)
    {
        return $"""<c r="{reference}" t="inlineStr"><is><t>{System.Security.SecurityElement.Escape(value)}</t></is></c>""";
    }

    private static string Row(int row, string cells)
    {
        return $"""<row r="{row}">{cells}</row>""";
    }

    private static string DataRow(int row, string productId, string serialNumber)
    {
        return Row(row, TextCell($"A{row}", productId) + TextCell($"B{row}", serialNumber));
    }

    private static Stream CreateWorkbook(string rows, string? sharedStrings = null)
    {
        MemoryStream stream = new MemoryStream();
        using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WritePart(archive, "xl/workbook.xml", """
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets><sheet name="Inventory" sheetId="1" r:id="rId1"/></sheets>
                </workbook>
                """);
            string stringsRelationship = sharedStrings == null ? "" :
                """<Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="sharedStrings.xml"/>""";
            WritePart(archive, "xl/_rels/workbook.xml.rels", $"""
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="/xl/worksheets/inventory.xml"/>
                  {stringsRelationship}
                </Relationships>
                """);
            WritePart(archive, "xl/worksheets/inventory.xml", $"""
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData>{rows}</sheetData></worksheet>
                """);
            if (sharedStrings != null)
            {
                WritePart(archive, "xl/sharedStrings.xml", $"""
                    <sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">{sharedStrings}</sst>
                    """);
            }
        }

        stream.Position = 0;
        return stream;
    }

    private static void WritePart(ZipArchive archive, string path, string content)
    {
        using StreamWriter writer = new StreamWriter(archive.CreateEntry(path).Open());
        writer.Write(content);
    }
}
