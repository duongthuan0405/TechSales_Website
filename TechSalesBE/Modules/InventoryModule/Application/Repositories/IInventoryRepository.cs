using InventoryModule.Domain.Entities;

namespace InventoryModule.Application.Repositories;

internal interface IInventoryRepository
{
    Task<Inventory?> GetStockAsync(Guid productId, CancellationToken cancellationToken);

    Task<ProductUnit?> GetProductUnitAsync(
        Guid productId, string serialNumber, CancellationToken cancellationToken);

    Task EnsureStockExistsAsync(Guid productId, CancellationToken cancellationToken);
    Task<Inventory?> GetStockForUpdateAsync(Guid productId, CancellationToken cancellationToken);
    Task UpdateStockQuantityAsync(Inventory stock, CancellationToken cancellationToken);
    void AddProductUnits(List<ProductUnit> units);
    Task UpdateProductUnitStatusAsync(ProductUnit unit, CancellationToken cancellationToken);
}
