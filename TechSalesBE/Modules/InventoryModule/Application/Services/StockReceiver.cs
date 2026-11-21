using InventoryModule.Application.Repositories;
using InventoryModule.Domain.Entities;

namespace InventoryModule.Application.Services;

internal sealed class StockReceiver
{
    private readonly IInventoryRepository _inventory;

    public StockReceiver(IInventoryRepository inventory)
    {
        _inventory = inventory;
    }

    public async Task<Inventory> ReceiveAsync(Guid productId, List<ProductUnit> units, CancellationToken cancellationToken)
    {
        await _inventory.EnsureStockExistsAsync(productId, cancellationToken);
        Inventory stock = await _inventory.GetStockForUpdateAsync(productId, cancellationToken)
            ?? throw new InvalidOperationException("The inventory row could not be loaded after initialization.");

        stock.Receive(units.Count);
        await _inventory.UpdateStockQuantityAsync(stock, cancellationToken);
        _inventory.AddProductUnits(units);

        return stock;
    }
}
