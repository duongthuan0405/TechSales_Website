using System;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class CartItemDbModel
{
    public Guid cart_id { get; set; }
    public Guid product_id { get; set; }
    public int quantity { get; set; }
    public DateTimeOffset created_at { get; set; }
    public DateTimeOffset updated_at { get; set; }

    // Navigation properties
    public CartDbModel cart { get; set; } = null!;
    public ProductDbModel product { get; set; } = null!;
}
