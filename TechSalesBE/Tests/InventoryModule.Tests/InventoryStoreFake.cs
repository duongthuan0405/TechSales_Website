using InventoryModule.Application.Repositories;
using InventoryModule.Application.Services;
using InventoryModule.Domain.Entities;

namespace InventoryModule.Tests;

internal sealed class InventoryStoreFake : IInventoryRepository, IExecuteAtomically
{
    public Dictionary<Guid, Inventory> Stocks { get; } = new();
    public Dictionary<(Guid ProductId, string SerialNumber), ProductUnit> Units { get; } = new();
    public List<Guid> LockedProducts { get; } = new();
    public int TransactionCount { get; private set; }

    public Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken)
    {
        TransactionCount++;
        return operation();
    }

    public Task<Inventory?> GetStockAsync(Guid productId, CancellationToken cancellationToken)
    {
        Inventory? stock = Stocks.GetValueOrDefault(productId);
        return Task.FromResult(stock == null ? null : new Inventory(productId, stock.Quantity, stock.ReservedQuantity));
    }

    public Task<ProductUnit?> GetProductUnitAsync(Guid productId, string serialNumber, CancellationToken cancellationToken)
    {
        ProductUnit? unit = Units.GetValueOrDefault((productId, serialNumber));
        return Task.FromResult(unit == null ? null : new ProductUnit(productId, serialNumber, unit.Status));
    }

    public Task EnsureStockExistsAsync(Guid productId, CancellationToken cancellationToken)
    {
        Stocks.TryAdd(productId, new Inventory(productId, 0, 0));
        return Task.CompletedTask;
    }

    public Task<Inventory?> GetStockForUpdateAsync(Guid productId, CancellationToken cancellationToken)
    {
        LockedProducts.Add(productId);
        return GetStockAsync(productId, cancellationToken);
    }

    public Task UpdateStockQuantityAsync(Inventory stock, CancellationToken cancellationToken)
    {
        Stocks[stock.ProductId] = stock;
        return Task.CompletedTask;
    }

    public void AddProductUnits(List<ProductUnit> units)
    {
        foreach (ProductUnit unit in units)
        {
            Units.Add((unit.ProductId, unit.SerialNumber), unit);
        }
    }

    public Task UpdateProductUnitStatusAsync(ProductUnit unit, CancellationToken cancellationToken)
    {
        Units[(unit.ProductId, unit.SerialNumber)] = unit;
        return Task.CompletedTask;
    }
}
