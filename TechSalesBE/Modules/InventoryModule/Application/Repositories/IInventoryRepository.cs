using InventoryModule.Domain.Entities;

namespace InventoryModule.Application.Repositories;

internal interface IInventoryRepository
{
    Task<Inventory?> GetStockAsync(Guid productId, CancellationToken cancellationToken);

    Task<ProductUnit?> GetProductUnitAsync(
        Guid productId, string serialNumber, CancellationToken cancellationToken);
}
