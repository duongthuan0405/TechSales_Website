namespace InventoryModule.Application.UseCases.ImportStock;

internal sealed record ImportStock_UCOutput(
    int ReceivedQuantity, string Status, List<ImportStock_UCOutput.ProductStock> Products)
{
    internal sealed record ProductStock(Guid ProductId, int ReceivedQuantity, int Quantity);
}
