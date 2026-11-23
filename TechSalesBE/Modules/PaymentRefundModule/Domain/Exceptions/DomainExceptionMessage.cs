using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Domain.Exceptions;

internal static class DomainExceptionMessage
{
    public static string CheckoutNotAllowed(TransactionStatus status)
    {
        return status switch
        {
            TransactionStatus.SUCCESS => "Giao dịch đã thanh toán thành công, không thể thanh toán lại.",
            TransactionStatus.FAILED => "Giao dịch đã thất bại, không thể tiếp tục thanh toán giao dịch này.",
            TransactionStatus.CANCELLED => "Giao dịch đã bị hủy, không thể tiếp tục thanh toán giao dịch này.",
            _ => "Trạng thái giao dịch thanh toán không hợp lệ."
        };
    }

    public const string PaymentUrlInvalid = "Liên kết thanh toán không hợp lệ.";
    public const string OnlinePaymentFailed = "Cổng thanh toán thông báo giao dịch không thành công.";
    public const string TransactionReferenceMismatch = "Mã giao dịch cổng thanh toán không khớp.";
    public const string OnlineMethodRequired = "Thao tác này chỉ áp dụng cho giao dịch dùng phương thức ONLINE.";
    public const string CashMethodRequired = "Thao tác này chỉ áp dụng cho giao dịch dùng phương thức CASH.";
    public const string PaymentNotPending = "Thao tác này yêu cầu giao dịch đang ở trạng thái chờ thanh toán.";
}
