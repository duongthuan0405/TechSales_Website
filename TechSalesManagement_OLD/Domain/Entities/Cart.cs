using System;
using System.Collections.Generic;
using System.Linq;
using TechSalesManagement.Domain.Common;

namespace TechSalesManagement.Domain.Entities;

public class Cart
{
    public Guid id { get; set; }
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? updatedAt { get; set; }

    public Guid userId { get; set; }
    public List<CartItem> items { get; set; } = new();

    public decimal totalPrice => items.Sum(i => (i.product?.price ?? 0) * i.quantity);
    public int totalItemsCount => items.Sum(i => i.quantity);

    public Cart(Guid userId)
    {
        this.userId = userId;
    }

    public Cart() { }
}
