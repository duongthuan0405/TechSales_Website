namespace InventoryModule.Application.UseCases.ReceiveStock;

internal sealed record ReceiveStock_UCOutput(Guid ProductId, int ReceivedQuantity, int Quantity, string Status);
