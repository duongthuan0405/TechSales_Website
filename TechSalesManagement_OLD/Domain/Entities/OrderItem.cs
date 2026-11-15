using System;

namespace TechSalesManagement.Domain.Entities;

public class OrderItem
{
    public Guid order_id { get; set; }
    public Guid product_id { get; set; }
    public decimal price { get; set; }
    public int quantity { get; set; }
    public Product? product { get; set; }

    public OrderItem(Guid orderId, Guid productId, decimal price, int quantity)
    {
        order_id = orderId;
        product_id = productId;
        this.price = price;
        this.quantity = quantity;
    }

    public OrderItem() { }
}
