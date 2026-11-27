namespace CatalogModule.Contracts;

public interface IProductCatalog
{
    Task<List<CatalogProduct>> GetProductsAsync(List<Guid> productIds, CancellationToken cancellationToken);
}

public sealed record CatalogProduct(Guid Id, string Name, string Description, long? Price, string Status,
    Guid BrandId, string BrandName, Guid CategoryId, string CategoryName, List<CatalogProductImage> Images);
public sealed record CatalogProductImage(Guid Id, string ImageUrl, bool IsPrimary);
