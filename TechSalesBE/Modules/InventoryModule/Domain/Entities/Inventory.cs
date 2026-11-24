using InventoryModule.Domain.Rules;

namespace InventoryModule.Domain.Entities;

internal sealed class Inventory
{
    public Guid ProductId { get; }
    public int Quantity { get; private set; }
    public int? ReservedQuantity { get; private set; }

    public Inventory(Guid productId, int quantity, int? reservedQuantity)
    {
        InventoryRules.EnsureProductId(productId);
        InventoryRules.EnsureStockQuantities(quantity, reservedQuantity);

        ProductId = productId;
        Quantity = quantity;
        ReservedQuantity = reservedQuantity;
    }

    public void Receive(int quantity)
    {
        InventoryRules.EnsurePositiveQuantity(quantity);
        InventoryRules.EnsureCapacity(Quantity, quantity);
        Quantity += quantity;
    }

    public void RemoveAvailableStock(int quantity)
    {
        InventoryRules.EnsurePositiveQuantity(quantity);
        InventoryRules.EnsureAvailableStock(Quantity, quantity);
        Quantity -= quantity;
    }

    public void Reserve(int quantity)
    {
        int reservedQuantity = ReservedQuantity ?? 0;
        InventoryRules.EnsurePositiveQuantity(quantity);
        InventoryRules.EnsureAvailableStock(Quantity, quantity);
        InventoryRules.EnsureCapacity(reservedQuantity, quantity);

        Quantity -= quantity;
        ReservedQuantity = reservedQuantity + quantity;
    }

    public void ReleaseReservation(int quantity)
    {
        int reservedQuantity = ReservedQuantity ?? 0;
        InventoryRules.EnsurePositiveQuantity(quantity);
        InventoryRules.EnsureReservedStock(reservedQuantity, quantity);
        InventoryRules.EnsureCapacity(Quantity, quantity);

        Quantity += quantity;
        ReservedQuantity = reservedQuantity - quantity;
    }

    public void ConfirmSale(int quantity)
    {
        int reservedQuantity = ReservedQuantity ?? 0;
        InventoryRules.EnsurePositiveQuantity(quantity);
        InventoryRules.EnsureReservedStock(reservedQuantity, quantity);
        ReservedQuantity = reservedQuantity - quantity;
    }
}
