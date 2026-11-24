using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using CommonModule.BusinessExceptions;
using InventoryModule.Application.Constants;
using InventoryModule.Application.Exceptions;
using InventoryModule.Application.Services;

namespace InventoryModule.Infrastructure.Services;

internal sealed class XlsxStockImportReader : IStockImportWorkbookReader
{
    private const int MaxXmlPartSize = 16 * 1024 * 1024;
    private const int MaxWorksheetRowNumber = 1_048_576;
    private static readonly Uri WorkbookUri = new Uri("https://xlsx.invalid/xl/workbook.xml");

    public List<StockImportRow> Read(Stream content, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using ZipArchive archive = new ZipArchive(content, ZipArchiveMode.Read, leaveOpen: true);
            XElement workbook = ReadPart(archive, "xl/workbook.xml", "workbook");
            XElement relationships = ReadPart(archive, "xl/_rels/workbook.xml.rels", "Relationships");
            XNamespace sheetNamespace = workbook.Name.Namespace;
            XElement? sheet = workbook.Element(sheetNamespace + "sheets")?.Elements(sheetNamespace + "sheet").FirstOrDefault();
            string? sheetId = sheet?.Attributes().FirstOrDefault(attribute => attribute.Name.LocalName == "id")?.Value;
            if (string.IsNullOrEmpty(sheetId))
            {
                throw InvalidWorkbook();
            }

            XName relationshipName = relationships.Name.Namespace + "Relationship";
            XElement? sheetRelationship = relationships.Elements(relationshipName).FirstOrDefault(relationship =>
                (string?)relationship.Attribute("Id") == sheetId && HasRelationshipType(relationship, "/worksheet"));
            XElement? stringsRelationship = relationships.Elements(relationshipName)
                .FirstOrDefault(relationship => HasRelationshipType(relationship, "/sharedStrings"));

            List<string> sharedStrings = [];
            if (stringsRelationship != null)
            {
                XElement strings = ReadPart(archive, PartPath(stringsRelationship), "sst");
                foreach (XElement item in strings.Elements(strings.Name.Namespace + "si"))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    sharedStrings.Add(ReadRichText(item));
                }
            }

            XElement worksheet = ReadPart(archive, PartPath(sheetRelationship), "worksheet");
            return ReadRows(worksheet, sharedStrings, cancellationToken);
        }
        catch (Exception exception) when (
            exception is InvalidDataException or IOException or XmlException or ArgumentOutOfRangeException)
        {
            // Malformed ZIP offsets can also cause an out-of-range seek on the upload stream.
            throw InvalidWorkbook();
        }
    }

    private static List<StockImportRow> ReadRows(
        XElement worksheet, List<string> sharedStrings, CancellationToken cancellationToken)
    {
        XNamespace ns = worksheet.Name.Namespace;
        XElement? sheetData = worksheet.Element(ns + "sheetData");
        List<StockImportRow> result = new List<StockImportRow>();
        int previousRowNumber = 0;
        bool productIdFirst = false;
        foreach (XElement row in sheetData?.Elements(ns + "row") ?? [])
        {
            cancellationToken.ThrowIfCancellationRequested();
            int rowNumber = previousRowNumber + 1;
            XAttribute? rowReference = row.Attribute("r");
            if (rowReference != null && (!int.TryParse(rowReference.Value, out rowNumber)
                || rowNumber <= previousRowNumber || rowNumber > MaxWorksheetRowNumber))
            {
                throw InvalidWorkbook();
            }

            if (previousRowNumber == 0 && rowNumber != 1)
            {
                throw FileError(BusinessExceptionMessage.InvalidImportHeaders);
            }

            previousRowNumber = rowNumber;
            (string first, string second) = ReadRow(row, rowNumber, sharedStrings);
            if (rowNumber == 1)
            {
                bool standardOrder = HeaderMatches(first, "ProductId") && HeaderMatches(second, "SerialNumber");
                bool reversedOrder = HeaderMatches(first, "SerialNumber") && HeaderMatches(second, "ProductId");
                if (!standardOrder && !reversedOrder)
                {
                    throw FileError(BusinessExceptionMessage.InvalidImportHeaders);
                }

                productIdFirst = standardOrder;
            }
            else if (!string.IsNullOrWhiteSpace(first) || !string.IsNullOrWhiteSpace(second))
            {
                if (result.Count == StockImportLimits.MaxDataRows)
                {
                    throw FileError(BusinessExceptionMessage.TooManyImportRows);
                }

                string productId = productIdFirst ? first : second;
                string serialNumber = productIdFirst ? second : first;
                result.Add(new StockImportRow(rowNumber, productId, serialNumber));
            }
        }

        if (previousRowNumber == 0)
        {
            throw FileError(BusinessExceptionMessage.InvalidImportHeaders);
        }

        if (result.Count == 0)
        {
            throw FileError(BusinessExceptionMessage.ImportRowsRequired);
        }

        return result;
    }

    private static (string First, string Second) ReadRow(XElement row, int rowNumber, List<string> sharedStrings)
    {
        XNamespace ns = row.Name.Namespace;
        string? first = null;
        string? second = null;
        int previousColumn = 0;
        foreach (XElement cell in row.Elements(ns + "c"))
        {
            string? reference = (string?)cell.Attribute("r");
            int column = previousColumn + 1;
            if (reference != null)
            {
                column = ReadColumn(reference);
            }

            previousColumn = column;
            string value = ReadCell(cell, rowNumber, sharedStrings);
            switch (column)
            {
                case 1 when first == null:
                    first = value;
                    break;
                case 2 when second == null:
                    second = value;
                    break;
                case 1 or 2:
                    throw InvalidWorkbook();
                default:
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        throw RowError(rowNumber, BusinessExceptionMessage.UnexpectedImportColumn);
                    }
                    break;
            }
        }

        return (first ?? string.Empty, second ?? string.Empty);
    }

    private static int ReadColumn(string reference)
    {
        if (reference.Length >= 2 && char.IsAsciiDigit(reference[1]) && reference[0] is 'A' or 'B')
        {
            return reference[0] - 'A' + 1;
        }

        return 3;
    }

    private static string ReadCell(XElement cell, int rowNumber, List<string> sharedStrings)
    {
        XNamespace ns = cell.Name.Namespace;
        if (cell.Element(ns + "f") != null)
        {
            throw RowError(rowNumber, BusinessExceptionMessage.InvalidImportCell);
        }

        string? type = (string?)cell.Attribute("t");
        XElement? inlineText = cell.Element(ns + "is");
        if (type == "inlineStr" && inlineText != null)
        {
            return ReadRichText(inlineText);
        }

        string? value = (string?)cell.Element(ns + "v");
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (type == "s" && int.TryParse(value, out int index) && index >= 0 && index < sharedStrings.Count)
        {
            return sharedStrings[index];
        }

        if (type is null or "n" or "str")
        {
            return value;
        }

        throw RowError(rowNumber, BusinessExceptionMessage.InvalidImportCell);
    }

    private static string ReadRichText(XElement element)
    {
        XNamespace ns = element.Name.Namespace;
        return string.Concat(element.Elements(ns + "t").Concat(element.Elements(ns + "r").Elements(ns + "t"))
            .Select(text => text.Value));
    }

    private static bool HeaderMatches(string value, string header)
    {
        return string.Equals(value.Trim(), header, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasRelationshipType(XElement relationship, string suffix)
    {
        string? type = (string?)relationship.Attribute("Type");
        return type != null && type.EndsWith(suffix, StringComparison.Ordinal);
    }

    private static string PartPath(XElement? relationship)
    {
        string? target = (string?)relationship?.Attribute("Target");
        if (string.IsNullOrEmpty(target) || (string?)relationship?.Attribute("TargetMode") == "External"
            || !Uri.TryCreate(WorkbookUri, target, out Uri? uri)
            || uri.Scheme != WorkbookUri.Scheme || uri.Host != WorkbookUri.Host
            || uri.Query.Length != 0 || uri.Fragment.Length != 0)
        {
            throw InvalidWorkbook();
        }

        return Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
    }

    private static XElement ReadPart(ZipArchive archive, string path, string rootName)
    {
        ZipArchiveEntry? entry = archive.GetEntry(path);
        if (entry == null || entry.Length > MaxXmlPartSize)
        {
            throw InvalidWorkbook();
        }

        using Stream content = entry.Open();
        using XmlReader reader = XmlReader.Create(content, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = MaxXmlPartSize
        });
        XElement? root = XDocument.Load(reader, LoadOptions.PreserveWhitespace).Root;
        if (root == null || root.Name.LocalName != rootName)
        {
            throw InvalidWorkbook();
        }

        return root;
    }

    private static BusinessException InvalidWorkbook()
    {
        return FileError(BusinessExceptionMessage.InvalidImportWorkbook);
    }

    private static BusinessException FileError(string message)
    {
        return new BusinessException(BusinessExceptionCode.BAD_REQUEST, message,
            new Dictionary<string, List<string>> { ["File"] = [message] });
    }

    private static BusinessException RowError(int rowNumber, string message)
    {
        return new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.InvalidImportRows,
            new Dictionary<string, List<string>> { [$"Rows[{rowNumber}]"] = [message] });
    }
}
