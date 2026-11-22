namespace CatalogModule.Domain.Entities;

internal sealed class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    public Category(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
