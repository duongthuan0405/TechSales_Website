using InventoryModule.Application.Constants;
using InventoryModule.Domain.Enums;
using InventoryModule.Domain.Exceptions;

namespace InventoryModule.Application.Exceptions;

internal static class BusinessExceptionMessage
{
    public const string ProductNotFound = "Không tìm thấy một hoặc nhiều sản phẩm trong Catalog.";
    public const string InvalidProductId = DomainExceptionMessage.InvalidProductId;
    public static readonly string InvalidSerialNumber = DomainExceptionMessage.InvalidSerialNumber;
    public const string SerialNumbersRequired = "Specify at least one serial number.";
    public const string DuplicateSerialNumbers = "Serial numbers must not be duplicated.";
    public const string SerialNumberAlreadyExists = "One or more serial numbers are already registered for this product.";
    public const string StockNotFound = "No inventory record was found for this product.";
    public const string ProductUnitNotFound = "No product unit was found for this serial number.";
    public const string InvalidManualStatus = "The status must be " + nameof(ProductUnitStatus.DAMAGED) + ".";
    public const string ImportFileRequired = "Upload a non-empty Excel workbook.";
    public const string InvalidImportFileType = "The import file must have the .xlsx extension.";
    public static readonly string ImportFileTooLarge =
        $"The import file must not exceed {StockImportLimits.MaxFileMegabytes} MiB.";
    public const string InvalidImportWorkbook = "The file is not a readable .xlsx workbook.";
    public const string InvalidImportHeaders =
        "Row 1 of the first worksheet must have ProductId and SerialNumber headers in columns A and B.";
    public const string ImportRowsRequired = "The first worksheet must contain at least one stock row.";
    public static readonly string TooManyImportRows =
        $"The first worksheet must not contain more than {StockImportLimits.MaxDataRows} stock rows.";
    public const string InvalidImportRows = "The Excel workbook contains invalid stock rows.";
    public const string UnexpectedImportColumn = "Only the ProductId and SerialNumber columns may contain data.";
    public const string InvalidImportCell = "Enter a cell value. Formulas, errors, and unsupported cell types are not accepted.";
    public const string DuplicateImportUnit = "This serial number is duplicated for the same product in the workbook.";
}
