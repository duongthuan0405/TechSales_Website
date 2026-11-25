using System;
using TechSalesManagement.Domain.Common;

namespace TechSalesManagement.Domain.Entities;

public class ProductImage
{
    public Guid id { get; set; }
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? updatedAt { get; set; }

    public Guid productId { get; set; }
    public string imageUrl { get; set; } = string.Empty;
    public bool isPrimary { get; set; }

    public ProductImage(Guid productId, string imageUrl, bool isPrimary = false)
    {
        this.productId = productId;
        this.imageUrl = imageUrl;
        this.isPrimary = isPrimary;
    }

    public ProductImage() { }
}
