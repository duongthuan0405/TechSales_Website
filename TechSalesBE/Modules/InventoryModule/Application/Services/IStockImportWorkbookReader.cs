namespace InventoryModule.Application.Services;

internal interface IStockImportWorkbookReader
{
    List<StockImportRow> Read(Stream content, CancellationToken cancellationToken);
}
