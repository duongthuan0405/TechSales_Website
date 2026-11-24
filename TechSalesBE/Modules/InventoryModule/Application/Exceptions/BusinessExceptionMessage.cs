using InventoryModule.Application.Constants;
using InventoryModule.Domain.Enums;
using InventoryModule.Domain.Exceptions;

namespace InventoryModule.Application.Exceptions;

internal static class BusinessExceptionMessage
{
    public const string InvalidUnitPage = "Trang phải lớn hơn 0, số dòng mỗi trang từ 1 đến 100.";
    public const string InvalidUnitFilter = "Trạng thái thiết bị không hợp lệ.";
    public const string ProductNotFound = "Không tìm thấy một hoặc nhiều sản phẩm trong danh mục sản phẩm.";
    public const string InvalidProductId = DomainExceptionMessage.InvalidProductId;
    public static readonly string InvalidSerialNumber = DomainExceptionMessage.InvalidSerialNumber;
    public const string SerialNumbersRequired = "Vui lòng nhập ít nhất một số serial.";
    public const string DuplicateSerialNumbers = "Các số serial trong yêu cầu không được trùng nhau.";
    public const string SerialNumberAlreadyExists = "Một hoặc nhiều số serial đã tồn tại trong kho của sản phẩm này.";
    public const string StockNotFound = "Không tìm thấy thông tin tồn kho của sản phẩm này.";
    public const string ProductUnitNotFound = "Không tìm thấy thiết bị có số serial này.";
    public const string InvalidManualStatus = "Trạng thái phải là DAMAGED (hỏng) hoặc IN_STOCK (trong kho).";
    public const string ImportFileRequired = "Vui lòng tải lên một tệp Excel không rỗng.";
    public const string InvalidImportFileType = "Tệp nhập kho phải có phần mở rộng .xlsx.";
    public static readonly string ImportFileTooLarge =
        $"Tệp nhập kho không được vượt quá {StockImportLimits.MaxFileMegabytes} MiB.";
    public const string InvalidImportWorkbook = "Không thể đọc tệp Excel .xlsx này.";
    public const string InvalidImportHeaders =
        "Dòng 1 của trang tính đầu tiên phải có tiêu đề ProductId và SerialNumber ở cột A và B.";
    public const string ImportRowsRequired = "Trang tính đầu tiên phải có ít nhất một dòng dữ liệu nhập kho.";
    public static readonly string TooManyImportRows =
        $"Trang tính đầu tiên không được vượt quá {StockImportLimits.MaxDataRows} dòng dữ liệu nhập kho.";
    public const string InvalidImportRows = "Tệp Excel có dòng dữ liệu nhập kho không hợp lệ.";
    public const string UnexpectedImportColumn = "Chỉ hai cột ProductId và SerialNumber được chứa dữ liệu.";
    public const string InvalidImportCell = "Vui lòng nhập giá trị cho ô. Không chấp nhận công thức, ô lỗi hoặc kiểu ô không được hỗ trợ.";
    public const string DuplicateImportUnit = "Số serial này bị trùng cho cùng một sản phẩm trong tệp Excel.";
}
