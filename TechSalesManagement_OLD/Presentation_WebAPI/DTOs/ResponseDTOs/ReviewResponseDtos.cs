using System;
using System.Collections.Generic;

namespace TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;

public class ReviewResponseItemDto
{
    public Guid id { get; set; }
    public Guid reviewId { get; set; }
    public Guid userId { get; set; }
    public string userName { get; set; } = string.Empty;
    public string content { get; set; } = string.Empty;
    public DateTimeOffset createdAt { get; set; }
}

public class ReviewItemResponseDto
{
    public Guid id { get; set; }
    public int rating { get; set; }
    public string? comment { get; set; }
    public ProfileResponseDto profile { get; set; } = null!;
    public DateTimeOffset createdAt { get; set; }
    public List<ReviewResponseItemDto> responses { get; set; } = new();
}

public class ProductReviewsResponseDto
{
    public decimal averageRating { get; set; }
    public int totalCount { get; set; }
    public int pageNumber { get; set; }
    public int pageSize { get; set; }
    public List<ReviewItemResponseDto> items { get; set; } = new();
}

public class ReviewStaffResponseDto : ReviewItemResponseDto
{
    public string? productName { get; set; }
    public string status { get; set; } = string.Empty;
    public string? violationReason { get; set; }
}
