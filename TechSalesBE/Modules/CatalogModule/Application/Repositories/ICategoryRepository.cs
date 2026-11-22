using CatalogModule.Domain.Entities;

namespace CatalogModule.Application.Repositories;

internal interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken);
}
