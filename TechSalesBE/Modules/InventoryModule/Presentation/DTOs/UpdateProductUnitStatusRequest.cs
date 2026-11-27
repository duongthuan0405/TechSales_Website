namespace InventoryModule.Presentation.DTOs;

public sealed record UpdateProductUnitStatusRequest(string? SerialNumber, string? Status);
