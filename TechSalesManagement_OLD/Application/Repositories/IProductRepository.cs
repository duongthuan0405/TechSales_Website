using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Application.Enums;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetProductsAsync(string? keyword, List<Guid>? categoryIds, SortOrder? sortOrder, ProductStatus? status);
    Task<Product?> GetByIdAsync(Guid id);
    Task MigrateProductsAsync(Guid oldCategoryId, Guid newCategoryId);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task<(List<Product> products, int totalCount)> GetAdminProductsAsync(string? keyword, Guid? categoryId, ProductStatus? status, int pageNumber, int pageSize);
}
