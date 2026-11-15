using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Application.Exceptions;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Common;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Services.Implementations;

public class ProductManagementService : IProductManagementService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public ProductManagementService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IInventoryRepository inventoryRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _inventoryRepository = inventoryRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<Product> CreateProductAsync(string name, string description, decimal price, string brand, Guid categoryId, int initialStock, List<ProductImage> images, Guid staffId)
    {
        var categoryExists = await _categoryRepository.ExistsAsync(categoryId);
        if (!categoryExists) throw new BadRequestException(MessageConstants.MSG75);

        try
        {
            await _unitOfWork.BeginAsync();

            var product = new Product.Builder()
                .WithBasicInfo(name, description, price, brand, categoryId)
                .WithImages(images)
                .WithInventory(initialStock)
                .Build();

            await _productRepository.AddAsync(product);

            var auditLog = new AuditLog(staffId, "CREATE_PRODUCT", "Products", product.id.ToString());
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();
            
            // Invalidate cache
            await _cacheService.RemoveByPrefixAsync("products:");
            
            return product;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateProductAsync(Guid productId, string name, string description, decimal price, string brand, Guid categoryId, List<ProductImage> images, Guid staffId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) throw new NotFoundException("Product not found.");

        var categoryExists = await _categoryRepository.ExistsAsync(categoryId);
        if (!categoryExists) throw new BadRequestException(MessageConstants.MSG75);

        try
        {
            await _unitOfWork.BeginAsync();

            var oldPrice = product.price;
            product.UpdateInfo(name, description, price, brand, categoryId);
            product.images = images;

            await _productRepository.UpdateAsync(product);

            if (oldPrice != price)
            {
                var auditLog = new AuditLog(staffId, "UPDATE_PRICE", "Products", productId.ToString())
                {
                    oldValues = System.Text.Json.JsonSerializer.Serialize(new { price = oldPrice }),
                    newValues = System.Text.Json.JsonSerializer.Serialize(new { price = price }),
                    affectedColumns = "price"
                };
                await _auditLogRepository.AddAsync(auditLog);
            }

            await _unitOfWork.FinishAsync();
            
            // Invalidate cache
            await _cacheService.RemoveByPrefixAsync("products:");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task DiscontinueProductAsync(Guid productId, Guid staffId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) throw new NotFoundException("Product not found.");

        try
        {
            await _unitOfWork.BeginAsync();

            product.Discontinue();
            await _productRepository.UpdateAsync(product);

            var auditLog = new AuditLog(staffId, "DISCONTINUE_PRODUCT", "Products", productId.ToString());
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();
            
            // Invalidate cache
            await _cacheService.RemoveByPrefixAsync("products:");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateInventoryAsync(Guid productId, int value, StockAdjustmentType type, Guid staffId)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventory == null) throw new NotFoundException("Inventory record not found.");

        try
        {
            await _unitOfWork.BeginAsync();

            var oldQuantity = inventory.quantity;
            int newQuantity = type == StockAdjustmentType.ADD ? oldQuantity + value : value;
            
            inventory.UpdateStock(newQuantity);
            await _inventoryRepository.UpdateStockAsync(productId, newQuantity);

            var auditLog = new AuditLog(staffId, "UPDATE_STOCK", "Inventory", productId.ToString())
            {
                oldValues = System.Text.Json.JsonSerializer.Serialize(new { quantity = oldQuantity }),
                newValues = System.Text.Json.JsonSerializer.Serialize(new { quantity = newQuantity }),
                affectedColumns = "quantity"
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();
            
            // Invalidate cache
            await _cacheService.RemoveByPrefixAsync("products:");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<(List<Product> products, int totalCount)> GetAdminProductsAsync(string? keyword, Guid? categoryId, ProductStatus? status, int pageNumber, int pageSize)
    {
        return await _productRepository.GetAdminProductsAsync(keyword, categoryId, status, pageNumber, pageSize);
    }
}
