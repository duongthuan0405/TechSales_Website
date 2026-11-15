using System;
using TechSalesManagement.Domain.Common;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Domain.Entities;

public class Voucher
{
    public Guid id { get; set; }
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? updatedAt { get; set; }

    public string code { get; set; } = string.Empty;
    public VoucherType type { get; set; }
    public decimal value { get; set; }
    public int maxUsage { get; set; }
    public int usedCount { get; set; }
    public decimal minOrderAmount { get; set; }
    public DateTimeOffset? startDate { get; set; }
    public DateTimeOffset? endDate { get; set; }
    public bool isActive { get; set; }

    public Voucher(string code, VoucherType type, decimal value)
    {
        this.code = code;
        this.type = type;
        this.value = value;
        this.isActive = true;
    }

    public Voucher() { }

    public bool IsValid(decimal orderAmount)
    {
        return isActive && 
               usedCount < maxUsage && 
               orderAmount >= minOrderAmount && 
               (!startDate.HasValue || startDate <= DateTimeOffset.UtcNow) && 
               (!endDate.HasValue || endDate >= DateTimeOffset.UtcNow);
    }

    public bool IsExpired()
    {
        return endDate.HasValue && endDate < DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        this.isActive = false;
        this.updatedAt = DateTimeOffset.UtcNow;
    }
}
