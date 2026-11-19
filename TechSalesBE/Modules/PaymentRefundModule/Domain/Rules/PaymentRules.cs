using PaymentRefundModule.Domain.Enums;
using CommonModule.Domain.Exceptions;
using PaymentRefundModule.Domain.Exceptions;

namespace PaymentRefundModule.Domain.Rules;

internal static class PaymentRules
{
    public static void EnsureCheckoutAllowed(TransactionMethodType methodType, TransactionStatus status)
    {
        if (methodType != TransactionMethodType.ONLINE)
        {
            throw new DomainException(DomainExceptionCode.OnlineMethodRequired, DomainExceptionMessage.OnlineMethodRequired);
        }
        if (!IsPending(status))
        {
            throw new DomainException(DomainExceptionCode.PaymentNotPending, DomainExceptionMessage.PaymentNotPending);
        }
    }

    public static bool IsPending(TransactionStatus status) => status == TransactionStatus.PENDING;
    public static bool IsPaid(TransactionStatus status) => status == TransactionStatus.SUCCESS;

    public static void EnsureReplacementAllowed(TransactionStatus status, TransactionMethodType methodType)
    {
        if (methodType != TransactionMethodType.CASH)
        {
            throw new DomainException(DomainExceptionCode.CashMethodRequired, DomainExceptionMessage.CashMethodRequired);
        }
        if (!IsPending(status))
        {
            throw new DomainException(DomainExceptionCode.PaymentNotPending, DomainExceptionMessage.PaymentNotPending);
        }
    }

    public static void EnsureCashConfirmationAllowed(TransactionMethodType methodType, TransactionStatus status)
    {
        if (methodType != TransactionMethodType.CASH)
        {
            throw new DomainException(DomainExceptionCode.CashMethodRequired, DomainExceptionMessage.CashMethodRequired);
        }
        if (status != TransactionStatus.PENDING && status != TransactionStatus.SUCCESS)
        {
            throw new DomainException(DomainExceptionCode.PaymentNotPending, DomainExceptionMessage.PaymentNotPending);
        }
    }

    public static bool ValidType(string? type)
    {
        return Enum.GetNames<PaymentType>().Any(name => string.Equals(name, type?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static bool ValidStatus(string? status)
    {
        return Enum.GetNames<TransactionStatus>().Any(name => string.Equals(name, status?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static bool HasValidAmount(long amount)
    {
        return amount > 0;
    }

    public static bool HasValidReference(PaymentType type, Guid refId)
    {
        return Enum.IsDefined(type) && refId != Guid.Empty;
    }
}
