using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Services.Interfaces;

public interface IProductManagementService
{
    Task<Product> CreateProductAsync(string name, string description, decimal price, string brand, Guid categoryId, int initialStock, List<ProductImage> images, Guid staffId);
    Task UpdateProductAsync(Guid productId, string name, string description, decimal price, string brand, Guid categoryId, List<ProductImage> images, Guid staffId);
    Task DiscontinueProductAsync(Guid productId, Guid staffId);
    Task UpdateInventoryAsync(Guid productId, int value, StockAdjustmentType type, Guid staffId);
    Task<(List<Product> products, int totalCount)> GetAdminProductsAsync(string? keyword, Guid? categoryId, ProductStatus? status, int pageNumber, int pageSize);
}
