namespace InventoryModule.Application.UseCases.UpdateProductUnitStatus;

internal sealed record UpdateProductUnitStatus_UCOutput(Guid ProductId, string SerialNumber, string Status, int Quantity);
