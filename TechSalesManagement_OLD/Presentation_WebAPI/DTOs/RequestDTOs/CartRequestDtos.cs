using System;

namespace TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;

public class AddToCartRequestDto
{
    public Guid productId { get; set; }
    public int quantity { get; set; }
}

public class UpdateCartItemRequestDto
{
    public int quantity { get; set; }
}
