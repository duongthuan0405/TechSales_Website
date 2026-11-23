namespace CatalogModule.Presentation.DTOs;

public sealed class CreateProductRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public long? Price { get; init; }
    public Guid BrandId { get; init; }
    public Guid CategoryId { get; init; }
}
