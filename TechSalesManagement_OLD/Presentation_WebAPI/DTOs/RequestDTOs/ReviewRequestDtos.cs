using System;

namespace TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;

public class AddReviewRequestDto
{
    public Guid orderId { get; set; }
    public Guid productId { get; set; }
    public int ratingStars { get; set; }
    public string? reviewComment { get; set; }
}
