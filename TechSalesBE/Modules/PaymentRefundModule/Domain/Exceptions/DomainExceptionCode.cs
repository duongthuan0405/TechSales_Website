namespace PaymentRefundModule.Domain.Exceptions;

internal static class DomainExceptionCode
{
    public const string OnlineMethodRequired = "PAYMENT_ONLINE_METHOD_REQUIRED";
    public const string CashMethodRequired = "PAYMENT_CASH_METHOD_REQUIRED";
    public const string PaymentNotPending = "PAYMENT_NOT_PENDING";
}
