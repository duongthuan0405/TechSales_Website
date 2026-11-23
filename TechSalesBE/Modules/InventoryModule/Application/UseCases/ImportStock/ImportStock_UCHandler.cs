using CommonModule.BusinessExceptions;
using InventoryModule.Application.Exceptions;
using InventoryModule.Application.Services;
using InventoryModule.Domain.Entities;
using InventoryModule.Domain.Enums;
using InventoryModule.Domain.Rules;
using MediatR;

namespace InventoryModule.Application.UseCases.ImportStock;

internal sealed class ImportStock_UCHandler : IRequestHandler<ImportStock_UCInput, ImportStock_UCOutput>
{
    private readonly IStockImportWorkbookReader _reader;
    private readonly StockReceiver _receiver;
    private readonly IExecuteAtomically _transaction;

    public ImportStock_UCHandler(
        IStockImportWorkbookReader reader, StockReceiver receiver, IExecuteAtomically transaction)
    {
        _reader = reader;
        _receiver = receiver;
        _transaction = transaction;
    }

    public async Task<ImportStock_UCOutput> Handle(ImportStock_UCInput input, CancellationToken cancellationToken)
    {
        List<StockImportRow> rows = _reader.Read(input.Content, cancellationToken);
        List<ProductUnit> units = ValidateRows(rows, cancellationToken);

        await _receiver.EnsureProductsExistAsync(units.Select(unit => unit.ProductId).Distinct().ToList(), cancellationToken);
        return await _transaction.ExecuteAsync(async () =>
        {
            List<ImportStock_UCOutput.ProductStock> products = new List<ImportStock_UCOutput.ProductStock>();
            foreach (IGrouping<Guid, ProductUnit> product in units.GroupBy(unit => unit.ProductId).OrderBy(group => group.Key))
            {
                List<ProductUnit> productUnits = product.ToList();
                Inventory stock = await _receiver.ReceiveAsync(product.Key, productUnits, cancellationToken);
                products.Add(new ImportStock_UCOutput.ProductStock(stock.ProductId, productUnits.Count, stock.Quantity));
            }

            return new ImportStock_UCOutput(units.Count, nameof(ProductUnitStatus.IN_STOCK), products);
        }, cancellationToken);
    }

    private static List<ProductUnit> ValidateRows(List<StockImportRow> rows, CancellationToken cancellationToken)
    {
        List<ProductUnit> units = new List<ProductUnit>();
        HashSet<(Guid ProductId, string SerialNumber)> seenUnits = new HashSet<(Guid, string)>();
        Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();
        foreach (StockImportRow row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool validProductId = Guid.TryParse(row.ProductId, out Guid productId)
                && InventoryRules.IsValidProductId(productId);
            bool validSerial = ProductUnitRules.IsValidSerialNumber(row.SerialNumber);
            if (!validProductId)
            {
                errors.Add($"Rows[{row.RowNumber}].ProductId", [BusinessExceptionMessage.InvalidProductId]);
            }

            if (!validSerial)
            {
                errors.Add($"Rows[{row.RowNumber}].SerialNumber", [BusinessExceptionMessage.InvalidSerialNumber]);
            }

            if (!validProductId || !validSerial)
            {
                continue;
            }

            if (!seenUnits.Add((productId, row.SerialNumber)))
            {
                errors.Add($"Rows[{row.RowNumber}].SerialNumber", [BusinessExceptionMessage.DuplicateImportUnit]);
                continue;
            }

            units.Add(new ProductUnit(productId, row.SerialNumber));
        }

        if (errors.Count > 0)
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.InvalidImportRows, errors);
        }

        return units;
    }
}
