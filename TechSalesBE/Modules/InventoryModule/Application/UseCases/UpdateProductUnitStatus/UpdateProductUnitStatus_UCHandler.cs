using CommonModule.BusinessExceptions;
using InventoryModule.Application.Exceptions;
using InventoryModule.Application.Repositories;
using InventoryModule.Application.Services;
using InventoryModule.Domain.Entities;
using MediatR;

namespace InventoryModule.Application.UseCases.UpdateProductUnitStatus;

internal sealed class UpdateProductUnitStatus_UCHandler
    : IRequestHandler<UpdateProductUnitStatus_UCInput, UpdateProductUnitStatus_UCOutput>
{
    private readonly IInventoryRepository _inventory;
    private readonly IExecuteAtomically _transaction;

    public UpdateProductUnitStatus_UCHandler(IInventoryRepository inventory, IExecuteAtomically transaction)
    {
        _inventory = inventory;
        _transaction = transaction;
    }

    public Task<UpdateProductUnitStatus_UCOutput> Handle(
        UpdateProductUnitStatus_UCInput input, CancellationToken cancellationToken)
    {
        return _transaction.ExecuteAsync(async () =>
        {
            Inventory stock = await _inventory.GetStockForUpdateAsync(input.ProductId, cancellationToken)
                ?? throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.StockNotFound);
            ProductUnit unit = await _inventory.GetProductUnitAsync(input.ProductId, input.SerialNumber, cancellationToken)
                ?? throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.ProductUnitNotFound);

            unit.MarkDamaged();
            stock.RemoveAvailableStock(1);
            await _inventory.UpdateProductUnitStatusAsync(unit, cancellationToken);
            await _inventory.UpdateStockQuantityAsync(stock, cancellationToken);

            return new UpdateProductUnitStatus_UCOutput(
                unit.ProductId, unit.SerialNumber, unit.Status.ToString(), stock.Quantity);
        }, cancellationToken);
    }
}
