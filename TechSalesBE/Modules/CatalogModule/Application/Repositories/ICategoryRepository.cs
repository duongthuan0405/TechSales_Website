using CatalogModule.Domain.Entities;

namespace CatalogModule.Application.Repositories;

internal interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task SaveAsync(Category category, CancellationToken cancellationToken);
}
