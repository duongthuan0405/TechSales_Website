using System;
using TechSalesManagement.Domain.Common;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Domain.Entities;

public class Payment
{
    public Guid id { get; set; }
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? updatedAt { get; set; }

    public Guid orderId { get; set; }
    public Guid paymentMethodId { get; set; }
    public PaymentStatus status { get; set; } = PaymentStatus.PENDING;
    public decimal amount { get; set; }
    public string? transactionRef { get; set; }

    public Payment(Guid orderId, Guid paymentMethodId, decimal amount)
    {
        this.orderId = orderId;
        this.paymentMethodId = paymentMethodId;
        this.amount = amount;
    }

    public Payment() { }
}
