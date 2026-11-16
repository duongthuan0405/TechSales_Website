using CatalogModule.Domain.Enums;

namespace CatalogModule.Infrastructure.Persistence.Models;

internal sealed class ProductDbModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long? Price { get; set; }
    public ProductStatus Status { get; set; }
    public Guid BrandId { get; set; }
    public Guid CategoryId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public BrandDbModel Brand { get; set; } = null!;
    public CategoryDbModel Category { get; set; } = null!;
    public ICollection<ProductImageDbModel> Images { get; set; } = new HashSet<ProductImageDbModel>();
}
