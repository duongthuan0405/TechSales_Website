using System;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class ProductImageDbModel
{
    public Guid id { get; set; }
    public Guid product_id { get; set; }
    public string image_url { get; set; } = string.Empty;
    public bool is_primary { get; set; }
    public DateTimeOffset created_at { get; set; }

    // Navigation properties
    public ProductDbModel product { get; set; } = null!;
}
