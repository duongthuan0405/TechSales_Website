using InventoryModule.Application.Repositories;
using InventoryModule.Domain.Entities;
using InventoryModule.Application.Ports;
using InventoryModule.Application.Exceptions;
using CommonModule.BusinessExceptions;

namespace InventoryModule.Application.Services;

internal sealed class StockReceiver
{
    private readonly IInventoryRepository _inventory;
    private readonly IInventoryProducts _products;

    public StockReceiver(IInventoryRepository inventory, IInventoryProducts products)
    {
        _inventory = inventory;
        _products = products;
    }

    public async Task EnsureProductsExistAsync(List<Guid> productIds, CancellationToken cancellationToken)
    {
        List<Guid> existing = await _products.GetExistingProductIdsAsync(productIds, cancellationToken);
        List<Guid> missing = productIds.Except(existing).ToList();
        if (missing.Count > 0)
        {
            throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.ProductNotFound,
                new Dictionary<string, List<string>> { ["ProductIds"] = missing.Select(id => id.ToString()).ToList() });
        }
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
