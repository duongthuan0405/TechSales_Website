using System;
using TechSalesManagement.Domain.Common;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Domain.Entities;

public class Review
{
    public Guid id { get; set; }
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? updatedAt { get; set; }

    public Guid? userId { get; set; }
    public Guid productId { get; set; }
    public int rating { get; set; }
    public string? comment { get; set; }
    public ReviewStatus status { get; set; } = ReviewStatus.VISIBLE;
    public string? violationReason { get; set; }
    public UserProfile? profile { get; set; }
    public List<ReviewResponse> responses { get; set; } = new();
    public string? productName { get; set; } // For staff view aggregation

    public Review(Guid? userId, Guid productId, int rating, string? comment)
    {
        this.userId = userId;
        this.productId = productId;
        this.rating = rating;
        this.comment = comment;
    }

    public Review() { }
}
