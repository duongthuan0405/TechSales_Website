namespace PaymentRefundModule.Application.Exceptions;

internal static class BusinessExceptionMessage
{
    public const string PaymentStatusInvalid = "Trạng thái thanh toán không hợp lệ.";
    public static string PaymentPaginationInvalid => $"Số trang phải lớn hơn 0, kích thước trang từ 1 đến {Constants.PaginationLimits.MaximumPageSize} và vị trí trang không vượt giới hạn cho phép.";
    public const string PaymentNotFound = "Không tìm thấy giao dịch thanh toán.";
    public const string PaymentIdInvalid = "Mã giao dịch thanh toán không hợp lệ.";
    public const string PaymentAccessDenied = "Bạn không có quyền xem giao dịch thanh toán này.";
    public const string PaymentNotOwned = "Bạn không có quyền thanh toán cho đối tượng này.";
    public const string PaymentAmountInvalid = "Số tiền thanh toán phải lớn hơn 0.";
    public const string CustomerIdInvalid = "Mã khách hàng không hợp lệ.";
    public const string PaymentTypeInvalid = "Loại thanh toán phải là ORDER hoặc REPAIR.";
    public const string PaymentReferenceInvalid = "Mã tham chiếu thanh toán không được để trống hoặc là GUID rỗng.";
    public const string TransactionMethodIdInvalid = "Mã phương thức giao dịch không hợp lệ.";
    public const string TransactionMethodNotFound = "Không tìm thấy phương thức giao dịch.";
}
