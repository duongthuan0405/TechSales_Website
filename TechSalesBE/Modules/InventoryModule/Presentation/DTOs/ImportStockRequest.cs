using Microsoft.AspNetCore.Http;

namespace InventoryModule.Presentation.DTOs;

public sealed record ImportStockRequest(IFormFile? File);
