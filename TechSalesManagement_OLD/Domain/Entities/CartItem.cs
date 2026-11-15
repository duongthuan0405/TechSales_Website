using System;

namespace TechSalesManagement.Domain.Entities;

public class CartItem
{
    public Guid cartId { get; set; }
    public Guid productId { get; set; }
    public int quantity { get; set; }
    public DateTimeOffset createdAt { get; set; }
    public DateTimeOffset updatedAt { get; set; }
    public Product? product { get; set; }

    public CartItem(Guid cartId, Guid productId, int quantity)
    {
        this.cartId = cartId;
        this.productId = productId;
        this.quantity = quantity;
        createdAt = DateTimeOffset.UtcNow;
        updatedAt = DateTimeOffset.UtcNow;
    }

    public CartItem() { }
}
