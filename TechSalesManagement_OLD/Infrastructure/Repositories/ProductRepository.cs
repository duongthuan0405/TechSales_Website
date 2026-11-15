using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Application.Enums;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;
using TechSalesManagement.Infrastructure.Persistence;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly TechSalesDbContext _dbContext;

    public ProductRepository(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Product>> GetProductsAsync(string? keyword, List<Guid>? categoryIds, SortOrder? sortOrder, ProductStatus? status)
    {
        var query = _dbContext.Products
            .Include(p => p.product_images)
            .Include(p => p.inventory)
            .Include(p => p.reviews)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(p => p.status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowercaseKeyword = keyword.ToLower();
            query = query.Where(p => p.name.ToLower().Contains(lowercaseKeyword) 
                                  || p.brand.ToLower().Contains(lowercaseKeyword));
        }

        if (categoryIds != null && categoryIds.Any())
        {
            query = query.Where(p => categoryIds.Contains(p.category_id));
        }

        if (sortOrder != null)
        {
            if (sortOrder == SortOrder.ASC)
            {
                query = query.OrderBy(p => p.price);
            }
            else if (sortOrder == SortOrder.DESC)
            {
                query = query.OrderByDescending(p => p.price);
            }
        }

        var dbModels = await query.ToListAsync();

        return dbModels.Select(MapToEntity).Cast<Product>().ToList();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        var dbModel = await _dbContext.Products
            .Include(p => p.product_images)
            .Include(p => p.inventory)
            .Include(p => p.reviews)
            .FirstOrDefaultAsync(p => p.id == id);

        return MapToEntity(dbModel);
    }

    public async Task MigrateProductsAsync(Guid oldCategoryId, Guid newCategoryId)
    {
        await _dbContext.Products
            .Where(p => p.category_id == oldCategoryId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.category_id, newCategoryId));
    }

    public async Task AddAsync(Product product)
    {
        var dbModel = new ProductDbModel
        {
            id = product.id,
            name = product.name,
            description = product.description,
            price = product.price,
            brand = product.brand,
            category_id = product.categoryId,
            status = product.status,
            created_at = product.createdAt,
            updated_at = product.updatedAt
        };

        if (product.inventory != null)
        {
            dbModel.inventory = new InventoryDbModel
            {
                product_id = product.id,
                quantity = product.inventory.quantity,
                reserved_quantity = product.inventory.reservedQuantity
            };
        }

        dbModel.product_images = product.images.Select(img => new ProductImageDbModel
        {
            id = Guid.NewGuid(),
            product_id = product.id,
            image_url = img.imageUrl,
            is_primary = img.isPrimary
        }).ToList();

        await _dbContext.Products.AddAsync(dbModel);
    }

    public async Task UpdateAsync(Product product)
    {
        var dbModel = await _dbContext.Products
            .Include(p => p.product_images)
            .FirstOrDefaultAsync(p => p.id == product.id);

        if (dbModel != null)
        {
            dbModel.name = product.name;
            dbModel.description = product.description;
            dbModel.price = product.price;
            dbModel.brand = product.brand;
            dbModel.category_id = product.categoryId;
            dbModel.status = product.status;
            dbModel.updated_at = DateTimeOffset.UtcNow;

            // Update images only if they have changed or to be safe
            // Remove existing images
            var existingImages = await _dbContext.ProductImages.Where(img => img.product_id == product.id).ToListAsync();
            _dbContext.ProductImages.RemoveRange(existingImages);
            
            // Add new images
            var newImages = product.images.Select(img => new ProductImageDbModel
            {
                id = Guid.NewGuid(),
                product_id = product.id,
                image_url = img.imageUrl,
                is_primary = img.isPrimary
            }).ToList();
            
            await _dbContext.ProductImages.AddRangeAsync(newImages);
        }
    }

    public async Task<(List<Product> products, int totalCount)> GetAdminProductsAsync(string? keyword, Guid? categoryId, ProductStatus? status, int pageNumber, int pageSize)
    {
        var query = _dbContext.Products
            .Include(p => p.product_images)
            .Include(p => p.inventory)
            .Include(p => p.reviews)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(p => p.name.Contains(keyword) || p.brand.Contains(keyword));

        if (categoryId.HasValue)
            query = query.Where(p => p.category_id == categoryId.Value);

        if (status.HasValue)
            query = query.Where(p => p.status == status.Value);

        int totalCount = await query.CountAsync();

        var dbModels = await query
            .OrderByDescending(p => p.created_at)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var entities = dbModels.Select(m => MapToEntity(m)!).ToList();

        return (entities, totalCount);
    }

    private Product? MapToEntity(ProductDbModel? dbModel)
    {
        if (dbModel == null) return null;

        var product = new Product
        {
            id = dbModel.id,
            name = dbModel.name,
            description = dbModel.description,
            price = dbModel.price,
            status = dbModel.status,
            brand = dbModel.brand,
            categoryId = dbModel.category_id,
            createdAt = dbModel.created_at,
            updatedAt = dbModel.updated_at,
            rating = dbModel.reviews != null && dbModel.reviews.Any() 
                     ? Math.Round(dbModel.reviews.Average(r => r.rating), 1) 
                     : 0
        };

        if (dbModel.product_images != null && dbModel.product_images.Any())
        {
            product.images = dbModel.product_images.Select(img => new ProductImage
            {
                id = img.id,
                productId = img.product_id,
                imageUrl = img.image_url,
                isPrimary = img.is_primary,
                createdAt = img.created_at
            }).ToList();
        }

        if (dbModel.inventory != null)
        {
            product.inventory = new Inventory
            {
                productId = dbModel.inventory.product_id,
                quantity = dbModel.inventory.quantity,
                reservedQuantity = dbModel.inventory.reserved_quantity
            };
        }

        return product;
    }
}
