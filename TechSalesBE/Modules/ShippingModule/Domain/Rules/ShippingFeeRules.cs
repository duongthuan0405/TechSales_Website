using CommonModule.Domain.Exceptions;
using ShippingModule.Domain.Exceptions;

namespace ShippingModule.Domain.Rules;

internal static class ShippingFeeRules
{
    internal static void EnsureValid(string provinceCode, long fee)
    {
        if (!ShippingRules.ValidAdministrativeCode(provinceCode) || !ShippingRules.ValidShippingFee(fee))
        {
            throw new DomainException(DomainExceptionCode.INVALID_SHIPPING_FEE, DomainExceptionMessage.ShippingFeeInvalid);
        }

    }
}
