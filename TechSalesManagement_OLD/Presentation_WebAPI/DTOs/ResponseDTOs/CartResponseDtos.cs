using System;
using System.Collections.Generic;

namespace TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;

public class CartProductResponseDto
{
    public Guid id { get; set; }
    public string name { get; set; } = string.Empty;
    public string brand { get; set; } = string.Empty;
    public decimal price { get; set; }
    public List<ProductImageResponseDto> images { get; set; } = new();
}

public class CartItemResponseDto
{
    public Guid productId { get; set; }
    public int quantity { get; set; }
    public DateTimeOffset createdAt { get; set; }
    public DateTimeOffset updatedAt { get; set; }
    public CartProductResponseDto? product { get; set; }
}

public class CartResponseDto
{
    public Guid userId { get; set; }
    public List<CartItemResponseDto> items { get; set; } = new();
    public decimal totalPrice { get; set; }
    public int totalItemsCount { get; set; }
}
