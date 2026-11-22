using CatalogModule.Domain.Exceptions;
using CatalogModule.Domain.Rules;
using CommonModule.Domain.Exceptions;

namespace CatalogModule.Domain.Entities;

internal sealed class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    public Category(Guid id, string? name)
    {
        if (!CategoryRules.ValidName(name))
        {
            throw new DomainException(DomainExceptionCode.INVALID_CATEGORY, DomainExceptionMessage.InvalidCategory);
        }

        Id = id;
        Name = name!;
    }

    public static Category Create(string? name)
    {
        return new Category(Guid.NewGuid(), name);
    }
}
