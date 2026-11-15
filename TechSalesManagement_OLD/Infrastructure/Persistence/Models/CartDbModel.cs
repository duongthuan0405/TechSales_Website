using System;
using System.Collections.Generic;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class CartDbModel
{
    public Guid id { get; set; }
    public Guid user_id { get; set; }
    public DateTimeOffset created_at { get; set; }

    // Navigation properties
    public UserDbModel user { get; set; } = null!;
    public ICollection<CartItemDbModel> cart_items { get; set; } = new HashSet<CartItemDbModel>();
}
