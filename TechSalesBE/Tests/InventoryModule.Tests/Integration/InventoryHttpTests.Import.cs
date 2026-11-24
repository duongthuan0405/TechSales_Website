using Xunit;
using static InventoryModule.Tests.Integration.InventoryWorkbook;

namespace InventoryModule.Tests.Integration;

public sealed partial class InventoryHttpTests
{
    [InventoryPostgresFact(DisplayName = "INV-XLSX-01 ImportsMultipleProductsAndPreservesReservations")]
    public async Task ImportsMultipleProductsAndPreservesReservations()
    {
        var x1 = Guid.NewGuid();
        var x2 = Guid.NewGuid();
        byte[] baseWorkbook = Workbook(DataRow(2, x1, "000042") + DataRow(3, x1, "SN-A002") + DataRow(4, x2, "SN-B001"));
        await SeedStock(x1, 10, 3);
        var response = await Import(baseWorkbook, _app.BusinessAdmin);
        Ok(response, "Stock imported.");
        Assert.Equal(3, response.Data.GetProperty("receivedQuantity").GetInt32());
        Assert.Equal("IN_STOCK", response.Data.GetProperty("status").GetString());
        var products = response.Data.GetProperty("products").EnumerateArray().ToDictionary(p => p.GetProperty("productId").GetGuid());
        Assert.Equal(2, products[x1].GetProperty("receivedQuantity").GetInt32());
        Assert.Equal(12, products[x1].GetProperty("quantity").GetInt32());
        Assert.Equal(1, products[x2].GetProperty("receivedQuantity").GetInt32());
        Assert.Equal(1, products[x2].GetProperty("quantity").GetInt32());
        Assert.Equal((12, (int?)3), await Stock(x1));
        Assert.Equal((1, (int?)0), await Stock(x2));
        Assert.Equal("IN_STOCK", await Status(x1, "000042"));
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-02 ReadsReorderedHeadersSharedStringsAndTheFirstSheet")]
    public async Task ReadsReorderedHeadersSharedStringsAndTheFirstSheet()
    {
        var id = Guid.NewGuid();
        string header = Row(1, Cell("A1", " SerialNumber ") + Cell("B1", "productid"));
        string row = Row(3, """<c r="A3" t="s"><v>0</v></c>""" + Cell("B3", id.ToString()));
        var workbook = Workbook("<row r=\"2\"/>" + row, header, secondSheet: true, sharedString: " Shared-01 ");
        Ok(await Import(workbook, _app.BusinessAdmin, "import.XLSX"), "Stock imported.");
        Assert.Equal((1, (int?)0), await Stock(id));
        Assert.Equal("IN_STOCK", await Status(id, " Shared-01 "));
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-03 AcceptsTheSameSerialForDifferentProductsInAWorkbook")]
    public async Task AcceptsTheSameSerialForDifferentProductsInAWorkbook()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        Ok(await Import(Workbook(DataRow(2, a, "SN-SHARED") + DataRow(3, b, "SN-SHARED")), _app.BusinessAdmin), "Stock imported.");
        Assert.Equal("IN_STOCK", await Status(a, "SN-SHARED"));
        Assert.Equal("IN_STOCK", await Status(b, "SN-SHARED"));
        Assert.Equal((1, (int?)0), await Stock(a));
        Assert.Equal((1, (int?)0), await Stock(b));
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-04 RejectsMissingEmptyOrInvalidUploads")]
    public async Task RejectsMissingEmptyOrInvalidUploads()
    {
        var x1 = Guid.NewGuid();
        var x2 = Guid.NewGuid();
        byte[] baseWorkbook = Workbook(DataRow(2, x1, "000042") + DataRow(3, x1, "SN-A002") + DataRow(4, x2, "SN-B001"));
        var count = await TotalUnits();
        Error(await Import(null, _app.BusinessAdmin), 400, "BAD_REQUEST", "File");
        Error(await Import([], _app.BusinessAdmin), 400, "BAD_REQUEST", "File");
        Error(await Import(baseWorkbook, _app.BusinessAdmin, "inventory.xls"), 400, "BAD_REQUEST", "File");
        Error(await Import("broken"u8.ToArray(), _app.BusinessAdmin), 400, "BAD_REQUEST", "File");
        Assert.Equal(count, await TotalUnits());
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-05 RejectsInvalidHeadersEmptyDataAndUnexpectedColumns")]
    public async Task RejectsInvalidHeadersEmptyDataAndUnexpectedColumns()
    {
        var count = await TotalUnits();
        var id = Guid.NewGuid();
        Error(await Import(Workbook(DataRow(2, id, "A"), Row(1, Cell("A1", "Something") + Cell("B1", "SerialNumber"))), _app.BusinessAdmin), 400, "BAD_REQUEST", "File");
        Error(await Import(Workbook(""), _app.BusinessAdmin), 400, "BAD_REQUEST", "File");
        Error(await Import(Workbook(Row(2, Cell("A2", id.ToString()) + Cell("B2", "A") + Cell("C2", "extra"))), _app.BusinessAdmin), 400, "BAD_REQUEST", "Rows[2]");
        Assert.Equal(count, await TotalUnits());
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-06 ReportsProductAndSerialErrorsAtTheirExcelRows")]
    public async Task ReportsProductAndSerialErrorsAtTheirExcelRows()
    {
        var id = Guid.NewGuid();
        foreach (string invalid in new[] { "NOT-A-UUID", Guid.Empty.ToString() })
        {
            byte[] workbook = Workbook(DataRow(2, id, "A") + Row(3, Cell("A3", invalid) + Cell("B3", "B")) + Row(4, Cell("A4", id.ToString())));
            var result = await Import(workbook, _app.BusinessAdmin);
            Error(result, 400, "BAD_REQUEST", "Rows[3].ProductId");
            Error(result, 400, "BAD_REQUEST", "Rows[4].SerialNumber");
            Assert.True(await Stock(id) == null, "valid prior row not applied");
        }
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-07 RejectsDuplicatePairsAfterParsingProductIds")]
    public async Task RejectsDuplicatePairsAfterParsingProductIds()
    {
        var id = Guid.NewGuid();
        byte[] workbook = Workbook(DataRow(2, id, "A") + Row(5, Cell("A5", id.ToString("N")) + Cell("B5", "A")));
        Error(await Import(workbook, _app.BusinessAdmin), 400, "BAD_REQUEST", "Rows[5].SerialNumber");
        Assert.True(await Stock(id) == null, "duplicate workbook rows no writes");
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-08 RejectsFormulaAndErrorCells")]
    public async Task RejectsFormulaAndErrorCells()
    {
        var id = Guid.NewGuid();
        foreach (string cell in new[] { """<c r="B2"><f>1+1</f><v>2</v></c>""", """<c r="B2" t="e"><v>#REF!</v></c>""" })
        {
            Error(await Import(Workbook(Row(2, Cell("A2", id.ToString()) + cell)), _app.BusinessAdmin), 400, "BAD_REQUEST", "Rows[2]");
        }
        Assert.True(await Stock(id) == null, "unsupported cells no writes");
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-09 EnforcesTheDataRowLimit")]
    public async Task EnforcesTheDataRowLimit()
    {
        var valid = Guid.NewGuid();
        var invalid = Guid.NewGuid();
        byte[] atLimit = Workbook(string.Concat(Enumerable.Range(2, 5000).Select(i => DataRow(i, valid, "L-" + i))));
        byte[] overLimit = Workbook(string.Concat(Enumerable.Range(2, 5001).Select(i => DataRow(i, invalid, "L-" + i))));
        Error(await Import(overLimit, _app.BusinessAdmin), 400, "BAD_REQUEST", "File");
        Assert.True(await Stock(invalid) == null, "5001 rows leave stock absent");
        var response = await Import(atLimit, _app.BusinessAdmin);
        Ok(response, "Stock imported.");
        Assert.Equal(5000, response.Data.GetProperty("receivedQuantity").GetInt32());
        Assert.Equal(5000, await UnitCount(valid));
        Assert.Equal((5000, (int?)0), await Stock(valid));
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-10A RejectsFilesAboveTheValidatorLimit")]
    public async Task RejectsFilesAboveTheValidatorLimit()
    {
        var count = await TotalUnits();
        Error(await Import(new byte[5 * 1024 * 1024 + 1], _app.BusinessAdmin), 400, "BAD_REQUEST", "File", details: "The import file must not exceed 5 MiB.");
        Assert.Equal(count, await TotalUnits());
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-10B RejectsRequestsAboveTheMultipartLimit")]
    public async Task RejectsRequestsAboveTheMultipartLimit()
    {
        var count = await TotalUnits();
        var response = await Import(new byte[5 * 1024 * 1024 + 65 * 1024], _app.BusinessAdmin, expectContinue: true);
        Assert.True(response.Code is 400 or 413, "oversized request must be rejected with a client error");
        Assert.Equal(count, await TotalUnits());
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-11 RollsBackAllGroupsWhenOneSerialConflicts")]
    public async Task RollsBackAllGroupsWhenOneSerialConflicts()
    {
        var earlier = Guid.NewGuid();
        var later = Guid.NewGuid();
        if (earlier.CompareTo(later) > 0)
        {
            (earlier, later) = (later, earlier);
        }
        Ok(await Receive(later, ["TAKEN"], _app.BusinessAdmin), "Stock received.");
        Error(await Import(Workbook(DataRow(2, earlier, "NEW") + DataRow(3, later, "TAKEN")), _app.BusinessAdmin), 409, "CONFLICT");
        Assert.True(await Stock(earlier) == null, "earlier product stock rolled back");
        Assert.Equal(0, await UnitCount(earlier));
        Assert.Equal((1, (int?)0), await Stock(later));
        Assert.Equal(1, await UnitCount(later));
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-12 RepeatingAnImportLeavesStockUnchanged")]
    public async Task RepeatingAnImportLeavesStockUnchanged()
    {
        var x1 = Guid.NewGuid();
        var x2 = Guid.NewGuid();
        byte[] baseWorkbook = Workbook(DataRow(2, x1, "000042") + DataRow(3, x1, "SN-A002") + DataRow(4, x2, "SN-B001"));
        await SeedStock(x1, 10, 3);
        Ok(await Import(baseWorkbook, _app.BusinessAdmin), "Stock imported.");
        Error(await Import(baseWorkbook, _app.BusinessAdmin), 409, "CONFLICT");
        Assert.Equal((12, (int?)3), await Stock(x1));
        Assert.Equal((1, (int?)0), await Stock(x2));
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-13 ImportsConcurrentWorkbooksWithReversedProductOrder")]
    public async Task ImportsConcurrentWorkbooksWithReversedProductOrder()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var files = await Task.WhenAll(
            Import(Workbook(DataRow(2, a, "A1") + DataRow(3, b, "B1")), _app.BusinessAdmin),
            Import(Workbook(DataRow(2, b, "B2") + DataRow(3, a, "A2")), _app.BusinessAdmin));
        foreach (var r in files)
        {
            Ok(r, "Stock imported.");
        }
        Assert.Equal((2, (int?)0), await Stock(a));
        Assert.Equal((2, (int?)0), await Stock(b));
        Assert.Equal(2, await UnitCount(a));
        Assert.Equal(2, await UnitCount(b));
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-14 RejectsUnauthorizedForbiddenAndNonMultipartImports")]
    public async Task RejectsUnauthorizedForbiddenAndNonMultipartImports()
    {
        var x1 = Guid.NewGuid();
        var x2 = Guid.NewGuid();
        byte[] baseWorkbook = Workbook(DataRow(2, x1, "000042") + DataRow(3, x1, "SN-A002") + DataRow(4, x2, "SN-B001"));
        int count = await TotalUnits();
        await AssertRestrictedAsync(credential => Import(baseWorkbook, credential));
        Assert.Equal(415, (await _app.SendAsync(HttpMethod.Post, "/api/inventory/import", _app.BusinessAdmin, new { file = "not multipart" })).Code);
        Assert.Equal(count, await TotalUnits());
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-15 RollsBackEarlierGroupsWhenALaterGroupOverflows")]
    public async Task RollsBackEarlierGroupsWhenALaterGroupOverflows()
    {
        var earlier = Guid.NewGuid();
        var later = Guid.NewGuid();
        if (earlier.CompareTo(later) > 0)
        {
            (earlier, later) = (later, earlier);
        }
        await SeedStock(later, int.MaxValue, 0);
        Error(await Import(Workbook(DataRow(2, earlier, "NEW") + DataRow(3, later, "EXTRA")), _app.BusinessAdmin),
            422, "INVENTORY.STOCK_QUANTITY_LIMIT_EXCEEDED");
        Assert.True(await Stock(earlier) == null, "first group rolled back on overflow");
        Assert.Equal(0, await UnitCount(earlier));
        Assert.Equal((int.MaxValue, (int?)0), await Stock(later));
        Assert.Equal(0, await UnitCount(later));
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-16 EnforcesUnicodeSerialLengthInWorkbookRows")]
    public async Task EnforcesUnicodeSerialLengthInWorkbookRows()
    {
        var id = Guid.NewGuid();
        foreach (var invalid in new[] { "", "  ", Repeat("😀", 101) })
        {
            Error(await Import(Workbook(DataRow(2, id, invalid)), _app.BusinessAdmin), 400, "BAD_REQUEST", "Rows[2].SerialNumber");
        }
        Assert.True(await Stock(id) == null, "invalid serial cells no writes");
        Ok(await Import(Workbook(DataRow(2, id, Repeat("😀", 100))), _app.BusinessAdmin), "Stock imported.");
        Assert.Equal(1, await UnitCount(id));
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-17 RejectsCorruptWorkbookPartsAndRelationships")]
    public async Task RejectsCorruptWorkbookPartsAndRelationships()
    {
        var id = Guid.NewGuid();
        byte[] valid = Workbook(DataRow(2, id, "STRUCTURE"));
        string workbookPath = "xl/workbook.xml";
        string relationshipsPath = "xl/_rels/workbook.xml.rels";
        string worksheetPath = "xl/worksheets/sheet1.xml";
        var malformed = new[]
        {
            ReplacePart(valid, workbookPath, null),
            ReplacePart(valid, relationshipsPath, null),
            ReplacePart(valid, workbookPath, "<other/>"),
            ReplacePart(valid, workbookPath, "<workbook><sheets/></workbook>"),
            ReplacePart(valid, workbookPath, "<workbook"),
            ReplacePart(valid, relationshipsPath, """<Relationships><Relationship Id="rId1" Type="urn:fixture/worksheet" TargetMode="External" Target="worksheets/sheet1.xml"/></Relationships>"""),
            ReplacePart(valid, relationshipsPath, """<Relationships><Relationship Id="rId1" Type="urn:fixture/worksheet"/></Relationships>"""),
            ReplacePart(valid, relationshipsPath, """<Relationships><Relationship Id="rId1" Type="urn:fixture/worksheet" Target="worksheets/sheet1.xml?query=1"/></Relationships>"""),
            ReplacePart(valid, worksheetPath, """<!DOCTYPE worksheet><worksheet><sheetData/></worksheet>"""),
            ReplacePart(valid, worksheetPath, "<worksheet>" + new string(' ', 16 * 1024 * 1024) + "</worksheet>")
        };
        foreach (var workbook in malformed)
        {
            Error(await Import(workbook, _app.BusinessAdmin), 400, "BAD_REQUEST", "File", details: "The file is not a readable .xlsx workbook.");
        }
        Assert.True(await Stock(id) == null, "malformed workbook no writes");
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-18 ValidatesRowReferencesAndInfersMissingReferences")]
    public async Task ValidatesRowReferencesAndInfersMissingReferences()
    {
        var id = Guid.NewGuid();
        var malformed = new[] { "<row r=\"x\"/>", "<row r=\"1048577\"/>", "<row r=\"1\"/>" };
        foreach (string rows in malformed)
        {
            Error(await Import(Workbook(rows), _app.BusinessAdmin), 400, "BAD_REQUEST", "File");
        }
        byte[] firstNotOne = Workbook("", Row(2, Cell("A2", "ProductId") + Cell("B2", "SerialNumber")));
        Error(await Import(firstNotOne, _app.BusinessAdmin), 400, "BAD_REQUEST", "File");
        string implicitRow = """<row><c t="str"><v>""" + id + """</v></c><c><v>17</v></c><c><v></v></c></row>""";
        Ok(await Import(Workbook(implicitRow), _app.BusinessAdmin), "Stock imported.");
        Assert.Equal("IN_STOCK", await Status(id, "17"));
    }

    [InventoryPostgresFact(DisplayName = "INV-XLSX-19 RejectsDuplicateCellsAndUnsupportedCellValues")]
    public async Task RejectsDuplicateCellsAndUnsupportedCellValues()
    {
        var id = Guid.NewGuid();
        Error(await Import(Workbook(Row(2, Cell("A2", id.ToString()) + Cell("B2", "A") + Cell("B2", "B"))), _app.BusinessAdmin),
            400, "BAD_REQUEST", "File");
        foreach (string raw in new[]
        {
            """<c r="B2" t="s"><v>-1</v></c>""",
            """<c r="B2" t="s"><v>5</v></c>""",
            """<c r="B2" t="b"><v>1</v></c>"""
        })
        {
            Error(await Import(Workbook(Row(2, Cell("A2", id.ToString()) + raw), sharedString: "unused"), _app.BusinessAdmin),
                400, "BAD_REQUEST", "Rows[2]");
        }
        Assert.True(await Stock(id) == null, "bad cells no writes");
    }
}
