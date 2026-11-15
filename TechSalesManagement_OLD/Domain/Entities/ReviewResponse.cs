using System;
using TechSalesManagement.Domain.Common;

namespace TechSalesManagement.Domain.Entities;

public class ReviewResponse
{
    public Guid id { get; set; }
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? updatedAt { get; set; }

    public Guid reviewId { get; set; }
    public Guid userId { get; set; }
    public string content { get; set; } = string.Empty;
    public string? userName { get; set; }

    public ReviewResponse(Guid reviewId, Guid userId, string content)
    {
        this.reviewId = reviewId;
        this.userId = userId;
        this.content = content;
    }

    public ReviewResponse() { }
}
