namespace InventoryModule.Presentation.DTOs;

public sealed record ReceiveStockRequest(List<string>? SerialNumbers);
