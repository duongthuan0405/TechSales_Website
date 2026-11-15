using System;
using TechSalesManagement.Domain.Common;

namespace TechSalesManagement.Domain.Entities;

public class ShippingAddress
{
    public Guid id { get; set; }
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? updatedAt { get; set; }

    public Guid userId { get; set; }
    public string province { get; set; } = string.Empty;
    public string ward { get; set; } = string.Empty;
    public string detail { get; set; } = string.Empty;
    public bool isDefault { get; set; }
    public DateTimeOffset? deletedAt { get; set; }

    public ShippingAddress(Guid userId, string province, string ward, string detail)
    {
        this.userId = userId;
        this.province = province;
        this.ward = ward;
        this.detail = detail;
    }

    public ShippingAddress() { }
}
