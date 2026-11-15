using System.ComponentModel.DataAnnotations;

namespace TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;

public class ReviewReplyRequestDto
{
    [Required]
    public string content { get; set; } = string.Empty;
}

public class ReviewHideRequestDto
{
    [Required]
    public string reason { get; set; } = string.Empty;
}

public class CreateReviewRequestDto
{
    [Required]
    public Guid orderId { get; set; }
    
    [Required]
    public Guid productId { get; set; }
    
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int ratingStars { get; set; }
    
    public string? reviewComment { get; set; }
}
