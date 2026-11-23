using CatalogModule.Domain.Entities;

namespace CatalogModule.Application.Repositories;

internal interface ICategoryRepository
{
    Task<(List<Category> Items, int TotalCount)> GetPageAsync(int pageNumber, int pageSize,
        string? keyword, CancellationToken cancellationToken);
    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task SaveAsync(Category category, CancellationToken cancellationToken);
}
