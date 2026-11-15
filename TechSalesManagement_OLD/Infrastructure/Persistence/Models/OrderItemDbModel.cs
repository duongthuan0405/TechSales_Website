using System;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class OrderItemDbModel
{
    public Guid order_id { get; set; }
    public Guid product_id { get; set; }
    public decimal price { get; set; }
    public int quantity { get; set; }

    // Navigation properties
    public OrderDbModel order { get; set; } = null!;
    public ProductDbModel product { get; set; } = null!;
}
