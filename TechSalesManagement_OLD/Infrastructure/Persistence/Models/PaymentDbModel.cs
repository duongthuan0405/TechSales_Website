using System;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class PaymentDbModel
{
    public Guid id { get; set; }
    public Guid order_id { get; set; }
    public Guid payment_method_id { get; set; }
    public PaymentStatus status { get; set; }
    public decimal amount { get; set; }
    public string? transaction_ref { get; set; }
    public DateTimeOffset created_at { get; set; }
    public DateTimeOffset updated_at { get; set; }

    // Navigation properties
    public OrderDbModel order { get; set; } = null!;
    public PaymentMethodDbModel payment_method { get; set; } = null!;
}
