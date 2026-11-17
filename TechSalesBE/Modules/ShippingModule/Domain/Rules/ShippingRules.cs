using ShippingModule.Domain.Constants;

namespace ShippingModule.Domain.Rules;

internal static class ShippingRules
{
    public static bool ValidShippingFee(long fee)
    {
        return fee >= ShippingLimits.MinimumShippingFee;
    }

    public static bool ValidAdministrativeCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        return code.Length <= ShippingLimits.AdministrativeCodeMaxLength
            && code == code.Trim();
    }

    public static bool ValidAddressDetail(string? detail)
    {
        if (string.IsNullOrWhiteSpace(detail))
        {
            return false;
        }

        return detail.Length <= ShippingLimits.AddressDetailMaxLength;
    }
}
