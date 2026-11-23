using System.IO.Compression;
using System.Security;
using System.Text;

namespace InventoryModule.Tests.Integration;

internal static class InventoryWorkbook
{
    internal static string Cell(string reference, string value) =>
        $"""<c r="{reference}" t="inlineStr"><is><t>{SecurityElement.Escape(value)}</t></is></c>""";

    internal static string Row(int number, string cells) => $"""<row r="{number}">{cells}</row>""";
    internal static string DataRow(int number, Guid product, string serial) => Row(number, Cell("A" + number, product.ToString()) + Cell("B" + number, serial));

    internal static byte[] Workbook(string rows, string? header = null, bool secondSheet = false, string? sharedString = null)
    {
        using var output = new MemoryStream();
        using (var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            string second = secondSheet ? """<sheet r:id="rId2"/>""" : "";
            Part("xl/workbook.xml", $"""<workbook xmlns:r="urn:fixture"><sheets><sheet r:id="rId1"/>{second}</sheets></workbook>""");
            string strings = sharedString == null ? "" : """<Relationship Id="rId3" Type="urn:fixture/sharedStrings" Target="sharedStrings.xml"/>""";
            string relation = secondSheet ? """<Relationship Id="rId2" Type="urn:fixture/worksheet" Target="worksheets/sheet2.xml"/>""" : "";
            Part("xl/_rels/workbook.xml.rels", $"""<Relationships><Relationship Id="rId1" Type="urn:fixture/worksheet" Target="worksheets/sheet1.xml"/>{relation}{strings}</Relationships>""");
            if (sharedString != null)
            {
                Part("xl/sharedStrings.xml", $"<sst><si><t>{SecurityElement.Escape(sharedString)}</t></si></sst>");
            }
            if (secondSheet)
            {
                Part("xl/worksheets/sheet2.xml", "<worksheet><sheetData/></worksheet>");
            }
            header ??= Row(1, Cell("A1", "ProductId") + Cell("B1", "SerialNumber"));
            Part("xl/worksheets/sheet1.xml", $"<worksheet><sheetData>{header}{rows}</sheetData></worksheet>");

            void Part(string path, string text)
            {
                using var writer = new StreamWriter(zip.CreateEntry(path).Open(), new UTF8Encoding(false));
                writer.Write(text);
            }
        }
        return output.ToArray();
    }

    internal static byte[] ReplacePart(byte[] bytes, string path, string? replacement)
    {
        using var source = new MemoryStream(bytes);
        using var input = new ZipArchive(source, ZipArchiveMode.Read);
        using var target = new MemoryStream();
        using (var output = new ZipArchive(target, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var entry in input.Entries)
            {
                if (entry.FullName != path)
                {
                    using var current = entry.Open();
                    using var other = output.CreateEntry(entry.FullName).Open();
                    current.CopyTo(other);
                }
            }
            if (replacement != null)
            {
                using var writer = new StreamWriter(output.CreateEntry(path).Open(), new UTF8Encoding(false));
                writer.Write(replacement);
            }
        }
        return target.ToArray();
    }
}
