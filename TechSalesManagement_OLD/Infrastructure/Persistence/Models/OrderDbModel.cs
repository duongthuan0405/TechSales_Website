using System;
using System.Collections.Generic;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class OrderDbModel
{
    public Guid id { get; set; }
    public Guid user_id { get; set; }
    public OrderStatus status { get; set; }
    public decimal total_product_amount { get; set; }
    public decimal shipping_fee { get; set; }
    public decimal discount_amount { get; set; }
    public decimal total_amount { get; set; }
    public string shipping_address_snapshot { get; set; } = string.Empty;
    public DateTimeOffset created_at { get; set; }
    public DateTimeOffset? updated_at { get; set; }
    public DateTimeOffset? approved_at { get; set; }
    public DateTimeOffset? shipped_at { get; set; }
    public DateTimeOffset? delivered_at { get; set; }

    // Navigation properties
    public UserDbModel user { get; set; } = null!;
    public ICollection<OrderItemDbModel> order_items { get; set; } = new HashSet<OrderItemDbModel>();
    public ICollection<OrderVoucherDbModel> order_vouchers { get; set; } = new HashSet<OrderVoucherDbModel>();
    public ICollection<PaymentDbModel> payments { get; set; } = new HashSet<PaymentDbModel>();
}
