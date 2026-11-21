using CatalogModule.Domain.Entities;

namespace CatalogModule.Application.Repositories;

internal interface IBrandRepository
{
    Task<Brand?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task SaveAsync(Brand brand, CancellationToken cancellationToken);
}
