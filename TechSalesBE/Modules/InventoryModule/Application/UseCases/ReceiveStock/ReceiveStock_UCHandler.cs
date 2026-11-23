using InventoryModule.Application.Services;
using InventoryModule.Domain.Entities;
using InventoryModule.Domain.Enums;
using MediatR;

namespace InventoryModule.Application.UseCases.ReceiveStock;

internal sealed class ReceiveStock_UCHandler : IRequestHandler<ReceiveStock_UCInput, ReceiveStock_UCOutput>
{
    private readonly StockReceiver _receiver;
    private readonly IExecuteAtomically _transaction;

    public ReceiveStock_UCHandler(StockReceiver receiver, IExecuteAtomically transaction)
    {
        _receiver = receiver;
        _transaction = transaction;
    }

    public async Task<ReceiveStock_UCOutput> Handle(ReceiveStock_UCInput input, CancellationToken cancellationToken)
    {
        List<ProductUnit> units = input.SerialNumbers
            .Select(serialNumber => new ProductUnit(input.ProductId, serialNumber))
            .ToList();

        await _receiver.EnsureProductsExistAsync(new List<Guid> { input.ProductId }, cancellationToken);
        return await _transaction.ExecuteAsync(async () =>
        {
            Inventory stock = await _receiver.ReceiveAsync(input.ProductId, units, cancellationToken);

            return new ReceiveStock_UCOutput(
                stock.ProductId, units.Count, stock.Quantity, nameof(ProductUnitStatus.IN_STOCK));
        }, cancellationToken);
    }
}
