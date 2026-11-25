using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;

public class CreateProductRequestDto
{
    [Required]
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    [Range(0, double.MaxValue)]
    public decimal price { get; set; }
    public string brand { get; set; } = string.Empty;
    [Required]
    public Guid categoryId { get; set; }
    [Range(0, int.MaxValue)]
    public int initialStock { get; set; }
    public List<IFormFile>? imageFiles { get; set; }
}

public class UpdateProductRequestDto
{
    [Required]
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    [Range(0, double.MaxValue)]
    public decimal price { get; set; }
    public string brand { get; set; } = string.Empty;
    [Required]
    public Guid categoryId { get; set; }
    public List<ProductImageRequestDto> images { get; set; } = new();
}

public class ProductImageRequestDto
{
    [Required]
    public string imageUrl { get; set; } = string.Empty;
    public bool isPrimary { get; set; }
}

public class UpdateInventoryRequestDto
{
    [Range(0, int.MaxValue)]
    public int value { get; set; }
    [Required]
    public StockAdjustmentType type { get; set; }
}

public class AdminProductSearchRequestDto
{
    public string? keyword { get; set; }
    public Guid? categoryId { get; set; }
    public ProductStatus? status { get; set; }
    public int pageNumber { get; set; } = 1;
    public int pageSize { get; set; } = 20;
}
