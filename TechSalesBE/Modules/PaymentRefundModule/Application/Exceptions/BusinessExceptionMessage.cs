namespace PaymentRefundModule.Application.Exceptions;

internal static class BusinessExceptionMessage
{
    public const string CheckoutInProgress = "Giao dịch đang được khởi tạo thanh toán. Vui lòng không gửi yêu cầu trùng.";
    public const string PaymentClientIpInvalid = "Không xác định được địa chỉ IP của khách hàng để tạo liên kết thanh toán.";
    public const string VnpayAmountInvalid = "Số tiền thanh toán VNPAY phải từ 1 đến 9.999.999.999 VNĐ.";
    public const string PaymentNotificationInvalid = "Thông báo thanh toán không khớp giao dịch hoặc không hợp lệ.";
    public const string MomoIpnInvalid = "Thông báo thanh toán MoMo không hợp lệ hoặc không khớp giao dịch.";
    public const string PaymentGatewayNotConfigured = "Cổng thanh toán chưa được tích hợp. Chưa thể tạo liên kết thanh toán.";
    public const string PaymentAlreadyPaid = "Đối tượng này đã được thanh toán thành công.";
    public const string PendingPaymentExists = "Đã có giao dịch đang chờ thanh toán. Vui lòng sử dụng hoặc thay thế giao dịch hiện tại.";
    public const string OnlineReplacementUnavailable = "Chưa thể thay thế giao dịch ONLINE đang chờ vì phiên thanh toán tại cổng chưa được vô hiệu hóa.";
    public const string MultiplePendingPayments = "Có nhiều giao dịch đang chờ cho cùng đối tượng. Vui lòng kiểm tra dữ liệu trước khi tiếp tục.";
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
