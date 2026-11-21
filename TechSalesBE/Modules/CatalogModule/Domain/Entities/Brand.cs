using CatalogModule.Domain.Exceptions;
using CatalogModule.Domain.Rules;
using CommonModule.Domain.Exceptions;

namespace CatalogModule.Domain.Entities;

internal sealed class Brand
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    public Brand(Guid id, string? name)
    {
        if (!BrandRules.ValidName(name))
        {
            throw new DomainException(DomainExceptionCode.INVALID_BRAND, DomainExceptionMessage.InvalidBrand);
        }

        Id = id;
        Name = name!;
    }

    public static Brand Create(string? name)
    {
        return new Brand(Guid.NewGuid(), name);
    }
}
