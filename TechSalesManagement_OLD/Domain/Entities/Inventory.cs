using System;

namespace TechSalesManagement.Domain.Entities;

public class Inventory
{
    public Guid productId { get; set; }
    public int quantity { get; set; }
    public int reservedQuantity { get; set; }

    public int availableQuantity => quantity - reservedQuantity;

    public Inventory(Guid productId, int quantity)
    {
        this.productId = productId;
        this.quantity = quantity;
        reservedQuantity = 0;
    }

    public Inventory() { }

    public void UpdateStock(int newQuantity)
    {
        if (newQuantity < 0) throw new InvalidOperationException("Stock quantity cannot be negative.");
        this.quantity = newQuantity;
    }
}
