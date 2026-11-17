using PromotionModule.Domain.Constants;
using PromotionModule.Domain.Entities;
using PromotionModule.Domain.Enums;

namespace PromotionModule.Domain.Rules;

internal static class VoucherRules
{
    public static string NormalizeCode(string code)
    {
        return code.Trim().ToUpperInvariant();
    }

    public static bool ValidCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        return NormalizeCode(code).Length <= PromotionLimits.VoucherCodeMaxLength;
    }

    public static bool ValidType(string? type)
    {
        return string.Equals(type, nameof(VoucherType.FIXED), StringComparison.OrdinalIgnoreCase)
            || string.Equals(type, nameof(VoucherType.PERCENT), StringComparison.OrdinalIgnoreCase);
    }

    public static VoucherType ParseType(string type)
    {
        return Enum.Parse<VoucherType>(type, ignoreCase: true);
    }

    public static bool ValidValue(string? type, long? value)
    {
        if (!value.HasValue || value.Value < PromotionLimits.MinimumDiscountValue || !ValidType(type))
        {
            return false;
        }

        if (ParseType(type!) == VoucherType.PERCENT)
        {
            return value.Value <= PromotionLimits.MaximumPercent;
        }

        return true;
    }

    public static bool ValidMaximumUsage(int? maximumUsage)
    {
        return !maximumUsage.HasValue || maximumUsage.Value >= PromotionLimits.MinimumUsageLimit;
    }

    public static bool CoversUsedCount(int? maximumUsage, int usedCount)
    {
        return !maximumUsage.HasValue || maximumUsage.Value >= usedCount;
    }

    public static bool ValidMinimumOrder(long? minimumOrderAmount)
    {
        return !minimumOrderAmount.HasValue || minimumOrderAmount.Value >= PromotionLimits.MinimumOrderAmount;
    }

    public static bool ValidDateRange(DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        return !startDate.HasValue || !endDate.HasValue || endDate.Value > startDate.Value;
    }

    public static bool ValidDefinition(string code, string type, long? value, int? maximumUsage,
        long? minimumOrderAmount, DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        return ValidCode(code) && ValidType(type) && ValidValue(type, value)
            && ValidMaximumUsage(maximumUsage) && ValidMinimumOrder(minimumOrderAmount)
            && ValidDateRange(startDate, endDate);
    }

    public static bool CodeAvailable(Voucher? existing, Guid? currentVoucherId)
    {
        return existing == null || existing.Id == currentVoucherId;
    }
}
