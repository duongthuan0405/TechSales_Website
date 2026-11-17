using CommonModule.Domain.Exceptions;
using ShippingModule.Domain.Entities;
using ShippingModule.Domain.Exceptions;

namespace ShippingModule.Domain.Rules;

internal static class DeliveryRules
{
    public static ShippingFee RequireConfiguredFee(ShippingFee? shippingFee)
    {
        if (shippingFee == null)
        {
            throw new DomainException(DomainExceptionCode.DELIVERY_NOT_SUPPORTED,
                DomainExceptionMessage.DeliveryUnsupported);
        }

        return shippingFee;
    }
}
