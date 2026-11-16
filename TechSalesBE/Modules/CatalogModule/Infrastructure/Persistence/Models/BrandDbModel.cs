namespace CatalogModule.Infrastructure.Persistence.Models;

internal sealed class BrandDbModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<ProductDbModel> Products { get; set; } = new HashSet<ProductDbModel>();
}
