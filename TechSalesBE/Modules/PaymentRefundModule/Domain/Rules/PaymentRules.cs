using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Domain.Rules;

internal static class PaymentRules
{
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
