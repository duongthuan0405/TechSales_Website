using InventoryModule.Application.Repositories;
using InventoryModule.Application.Services;
using InventoryModule.Domain.Entities;
using InventoryModule.Domain.Enums;
using MediatR;

namespace InventoryModule.Application.UseCases.ReceiveStock;

internal sealed class ReceiveStock_UCHandler : IRequestHandler<ReceiveStock_UCInput, ReceiveStock_UCOutput>
{
    private readonly IInventoryRepository _inventory;
    private readonly IExecuteAtomically _transaction;

    public ReceiveStock_UCHandler(IInventoryRepository inventory, IExecuteAtomically transaction)
    {
        _inventory = inventory;
        _transaction = transaction;
    }

    public Task<ReceiveStock_UCOutput> Handle(ReceiveStock_UCInput input, CancellationToken cancellationToken)
    {
        List<ProductUnit> units = input.SerialNumbers
            .Select(serialNumber => new ProductUnit(input.ProductId, serialNumber))
            .ToList();

        return _transaction.ExecuteAsync(async () =>
        {
            await _inventory.EnsureStockExistsAsync(input.ProductId, cancellationToken);
            Inventory stock = await _inventory.GetStockForUpdateAsync(input.ProductId, cancellationToken)
                ?? throw new InvalidOperationException("The inventory row could not be loaded after initialization.");

            stock.Receive(units.Count);
            await _inventory.UpdateStockQuantityAsync(stock, cancellationToken);
            _inventory.AddProductUnits(units);

            return new ReceiveStock_UCOutput(
                stock.ProductId, units.Count, stock.Quantity, nameof(ProductUnitStatus.IN_STOCK));
        }, cancellationToken);
    }
}
