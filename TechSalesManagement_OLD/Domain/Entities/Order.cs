using System;
using TechSalesManagement.Domain.Common;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Domain.Entities;

public class Order
{
    public Guid id { get; set; }
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? updatedAt { get; set; }
    public DateTimeOffset? approvedAt { get; set; }
    public DateTimeOffset? shippedAt { get; set; }
    public DateTimeOffset? deliveredAt { get; set; }

    public Guid userId { get; set; }
    public OrderStatus status { get; set; } = OrderStatus.PENDING;
    public decimal totalProductAmount { get; set; }
    public decimal shippingFee { get; set; }
    public decimal discountAmount { get; set; }
    public decimal totalAmount { get; set; }
    public string shippingAddressSnapshot { get; set; } = string.Empty;

    public List<OrderItem> items { get; set; } = new();
    public List<Voucher> vouchers { get; set; } = new();

    public Order(Guid userId, decimal totalAmount, string addressSnapshot)
    {
        this.userId = userId;
        this.totalAmount = totalAmount;
        this.shippingAddressSnapshot = addressSnapshot;
    }

    public Order() { }

    public void Approve()
    {
        if (this.status != OrderStatus.PENDING) return;
        this.status = OrderStatus.APPROVED;
        this.approvedAt = DateTimeOffset.UtcNow;
        this.updatedAt = DateTimeOffset.UtcNow;
    }

    public void Ship()
    {
        if (this.status != OrderStatus.APPROVED) return;
        this.status = OrderStatus.SHIPPING;
        this.shippedAt = DateTimeOffset.UtcNow;
        this.updatedAt = DateTimeOffset.UtcNow;
    }

    public void Deliver()
    {
        if (this.status != OrderStatus.SHIPPING) return;
        this.status = OrderStatus.DELIVERED;
        this.deliveredAt = DateTimeOffset.UtcNow;
        this.updatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (this.status == OrderStatus.DELIVERED) return;
        this.status = OrderStatus.CANCELLED;
        this.updatedAt = DateTimeOffset.UtcNow;
    }
}
