namespace PaymentRefundModule.Domain.Exceptions;

internal static class DomainExceptionMessage
{
    public const string OnlinePaymentFailed = "Cổng thanh toán thông báo giao dịch không thành công.";
    public const string TransactionReferenceMismatch = "Mã giao dịch cổng thanh toán không khớp.";
    public const string OnlineMethodRequired = "Thao tác này chỉ áp dụng cho giao dịch dùng phương thức ONLINE.";
    public const string CashMethodRequired = "Thao tác này chỉ áp dụng cho giao dịch dùng phương thức CASH.";
    public const string PaymentNotPending = "Thao tác này yêu cầu giao dịch đang ở trạng thái chờ thanh toán.";
}
