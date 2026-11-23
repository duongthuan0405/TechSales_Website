using CatalogModule.Contracts;
using CatalogModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CatalogModule.Infrastructure.Services;

internal sealed class ProductCatalog : IProductCatalog
{
    private readonly CatalogDbContext _db;

    public ProductCatalog(CatalogDbContext db)
    {
        _db = db;
    }

    public async Task<List<CatalogProduct>> GetProductsAsync(List<Guid> productIds, CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
        {
            return new List<CatalogProduct>();
        }
        return await _db.Products.AsNoTracking().Where(product => productIds.Contains(product.Id))
            .Select(product => new CatalogProduct(product.Id, product.Name, product.Description, product.Price,
                product.Status.ToString(), product.BrandId, product.Brand.Name, product.CategoryId, product.Category.Name,
                product.Images.OrderBy(image => image.CreatedAt).ThenBy(image => image.Id)
                    .Select(image => new CatalogProductImage(image.Id, image.ImageUrl, image.IsPrimary ?? false)).ToList()))
            .ToListAsync(cancellationToken);
    }
}
