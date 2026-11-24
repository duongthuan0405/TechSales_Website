using InventoryModule.Domain.Entities;

namespace InventoryModule.Application.Repositories;

internal interface IProductUnitReader
{
    Task<(List<ProductUnit> Items, int TotalCount)> BrowseAsync(
        Guid? productId, string? keyword, string? status, int pageNumber, int pageSize,
        CancellationToken cancellationToken);
}
