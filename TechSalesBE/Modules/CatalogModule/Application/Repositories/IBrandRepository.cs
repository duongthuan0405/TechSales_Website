using CatalogModule.Domain.Entities;

namespace CatalogModule.Application.Repositories;

internal interface IBrandRepository
{
    Task<(List<Brand> Items, int TotalCount)> GetPageAsync(int pageNumber, int pageSize,
        string? keyword, CancellationToken cancellationToken);
    Task<List<Brand>> GetAllAsync(CancellationToken cancellationToken);
    Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Brand?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task SaveAsync(Brand brand, CancellationToken cancellationToken);
}
