using System;

namespace TechSalesManagement.Application.Services.Params;

public class AddReviewParams
{
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int RatingStars { get; set; }
    public string? ReviewComment { get; set; }
}

public class GetProductReviewsParams
{
    public Guid ProductId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
