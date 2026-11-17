using ShippingModule.Domain.Exceptions;
using CommonModule.Domain.Exceptions;
using ShippingModule.Domain.Entities;

namespace ShippingModule.Domain.Rules;

internal static class AddressOwnershipRules
{
    public static ShippingAddress EnsureOwned(ShippingAddress? address, Guid actorId)
    {
        if (address == null)
        {
            throw new DomainException(DomainExceptionCode.RULE_ADDRESS_NOT_FOUND, DomainExceptionMessage.AddressNotFound);
        }

        if (address.UserId != actorId)
        {
            throw new DomainException(DomainExceptionCode.RULE_ADDRESS_NOT_OWNED, DomainExceptionMessage.AddressNotOwned);
        }

        return address;
    }
}
