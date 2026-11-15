using System;
using System.Collections.Generic;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class ProductDbModel
{
    public Guid id { get; set; }
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public decimal price { get; set; }
    public ProductStatus status { get; set; } = ProductStatus.ACTIVE;
    public string brand { get; set; } = string.Empty;
    public Guid category_id { get; set; }
    public DateTimeOffset created_at { get; set; }
    public DateTimeOffset? updated_at { get; set; }

    // Navigation properties
    public CategoryDbModel category { get; set; } = null!;
    public InventoryDbModel? inventory { get; set; }
    public ICollection<ProductImageDbModel> product_images { get; set; } = new HashSet<ProductImageDbModel>();
    public ICollection<OrderItemDbModel> order_items { get; set; } = new HashSet<OrderItemDbModel>();
    public ICollection<CartItemDbModel> cart_items { get; set; } = new HashSet<CartItemDbModel>();
    public ICollection<ReviewDbModel> reviews { get; set; } = new HashSet<ReviewDbModel>();
}
