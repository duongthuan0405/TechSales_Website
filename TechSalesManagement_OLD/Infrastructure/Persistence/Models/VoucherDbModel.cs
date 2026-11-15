using System;
using System.Collections.Generic;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class VoucherDbModel
{
    public Guid id { get; set; }
    public string code { get; set; } = string.Empty;
    public VoucherType type { get; set; }
    public decimal value { get; set; }
    public int max_usage { get; set; }
    public int used_count { get; set; }
    public decimal min_order_amount { get; set; }
    public DateTimeOffset? start_date { get; set; }
    public DateTimeOffset? end_date { get; set; }
    public bool is_active { get; set; }
    public DateTimeOffset created_at { get; set; }
    public DateTimeOffset? updated_at { get; set; }

    // Navigation collections
    public ICollection<OrderVoucherDbModel> order_vouchers { get; set; } = new HashSet<OrderVoucherDbModel>();
}
