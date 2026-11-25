using System;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class InventoryDbModel
{
    public Guid product_id { get; set; }
    public int quantity { get; set; }
    public int reserved_quantity { get; set; }

    // Navigation properties
    public ProductDbModel product { get; set; } = null!;
}
