using System;
using System.Collections.Generic;
using TechSalesManagement.Application.Enums;

namespace TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;

public class ProductImageResponseDto
{
    public Guid id { get; set; }
    public string imageUrl { get; set; } = string.Empty;
    public bool isPrimary { get; set; }
}

public class ProductResponseDto
{
    public Guid id { get; set; }
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public decimal price { get; set; }
    public string brand { get; set; } = string.Empty;
    public Guid categoryId { get; set; }
    public List<ProductImageResponseDto> images { get; set; } = new();
    public StockStatus stockStatus { get; set; }
    public int availableQuantity { get; set; }
    public string status { get; set; } = string.Empty;
    public double rating { get; set; }
}

public class ProductDetailResponseDto
{
    public Guid id { get; set; }
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public decimal price { get; set; }
    public string brand { get; set; } = string.Empty;
    public Guid categoryId { get; set; }
    public List<ProductImageResponseDto> images { get; set; } = new();
    public StockStatus stockStatus { get; set; }
    public int availableQuantity { get; set; }
    public string status { get; set; } = string.Empty;
    public double rating { get; set; }
}
