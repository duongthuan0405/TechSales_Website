namespace InventoryModule.Infrastructure.Persistence.Models;

internal sealed class InvetoryDbModel
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public int? ReservedQuantity { get; set; }
}
