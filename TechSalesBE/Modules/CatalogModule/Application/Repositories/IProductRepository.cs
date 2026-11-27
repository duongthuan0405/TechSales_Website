using CatalogModule.Domain.Entities;
using CatalogModule.Domain.Enums;

namespace CatalogModule.Application.Repositories;

internal interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task UpdateAsync(Product product, CancellationToken cancellationToken);
    Task UpdateImagesAsync(Product product, CancellationToken cancellationToken);
    Task<(List<Product> Items, int TotalCount)> GetPageAsync(int pageNumber, int pageSize,
        string? keyword, List<Guid> keywordBrandIds, Guid? categoryId, ProductStatus? status,
        Guid? productId, Guid? brandId, CancellationToken cancellationToken,
        long? minPrice = null, long? maxPrice = null, string sortBy = "newest",
        Dictionary<Guid, double>? ratings = null);
}
