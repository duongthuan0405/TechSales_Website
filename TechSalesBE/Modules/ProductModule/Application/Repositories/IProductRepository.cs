using ProductModule.Domain.Entities;
using ProductModule.Domain.Enums;

namespace ProductModule.Application.Repositories;

internal interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task UpdateAsync(Product product, CancellationToken cancellationToken);
    Task<(List<Product> Items, int TotalCount)> GetPageAsync(int pageNumber, int pageSize,
        string? keyword, List<Guid> keywordBrandIds, Guid? categoryId, ProductStatus? status,
        Guid? productId, CancellationToken cancellationToken);
}
