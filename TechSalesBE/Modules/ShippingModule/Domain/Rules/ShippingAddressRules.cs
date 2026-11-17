using CommonModule.Domain.Exceptions;
using ShippingModule.Domain.Entities;
using ShippingModule.Domain.Exceptions;

namespace ShippingModule.Domain.Rules;

internal static class ShippingAddressRules
{
    internal static void EnsureValidIdentity(Guid id, Guid userId)
    {
        if (id == Guid.Empty || userId == Guid.Empty)
        {
            throw new DomainException(DomainExceptionCode.INVALID_ADDRESS, DomainExceptionMessage.AddressIdentityInvalid);
        }

    }

    internal static void EnsureNotDeleted(DateTimeOffset? deletedAt)
    {
        if (deletedAt.HasValue)
        {
            throw new DomainException(DomainExceptionCode.ADDRESS_DELETED, DomainExceptionMessage.AddressDeleted);
        }
    }

    internal static void EnsureValidAddress(Ward ward, string detail)
    {
        bool validWard = ward != null && ShippingRules.ValidAdministrativeCode(ward.Code);
        if (!validWard || !ShippingRules.ValidAddressDetail(detail))
        {
            throw new DomainException(DomainExceptionCode.INVALID_ADDRESS, DomainExceptionMessage.AddressInvalid);
        }
    }
}
