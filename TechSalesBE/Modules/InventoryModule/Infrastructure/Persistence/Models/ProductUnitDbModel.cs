using InventoryModule.Domain.Enums;

namespace InventoryModule.Infrastructure.Persistence.Models;

internal sealed class ProductUnitDbModel
{
    public Guid ProductId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public ProductUnitStatus Status { get; set; }
}
