using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TechSalesManagement.Application.Exceptions;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Common;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Category> CreateCategoryAsync(string name, Guid staffId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Category name is required.");
        }

        // BR165: Name uniqueness
        var existing = await _categoryRepository.GetByNameAsync(name);
        if (existing != null)
        {
            throw new BadRequestException(MessageConstants.MSG73);
        }

        try
        {
            await _unitOfWork.BeginAsync();

            var category = new Category(name);
            await _categoryRepository.AddAsync(category);

            var auditLog = new AuditLog(staffId, "CREATE_CATEGORY", "Categories", category.id.ToString())
            {
                newValues = System.Text.Json.JsonSerializer.Serialize(new { name = category.name })
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();
            
            // Invalidate cache
            await _cacheService.RemoveAsync("categories:all");
            
            return category;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteCategoryAsync(Guid id, Guid replacementCategoryId, Guid staffId)
    {
        if (id == replacementCategoryId)
        {
            throw new BadRequestException("Replacement category cannot be the same as the one being deleted.");
        }

        var categoryToDelete = await _categoryRepository.GetByIdAsync(id);
        if (categoryToDelete == null)
        {
            throw new NotFoundException("Category to delete not found.");
        }

        var replacementExists = await _categoryRepository.ExistsAsync(replacementCategoryId);
        if (!replacementExists)
        {
            throw new BadRequestException(MessageConstants.MSG75);
        }

        try
        {
            await _unitOfWork.BeginAsync();

            // UC-41: Migrate products before deletion
            await _productRepository.MigrateProductsAsync(id, replacementCategoryId);

            // Delete the old category
            await _categoryRepository.DeleteAsync(id);

            var auditLog = new AuditLog(staffId, "DELETE_CATEGORY", "Categories", id.ToString())
            {
                oldValues = System.Text.Json.JsonSerializer.Serialize(new { name = categoryToDelete.name }),
                newValues = System.Text.Json.JsonSerializer.Serialize(new { migratedTo = replacementCategoryId }),
                affectedColumns = "ALL"
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();

            // Invalidate cache
            await _cacheService.RemoveAsync("categories:all");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        var cacheKey = "categories:all";
        var cached = await _cacheService.GetAsync<List<Category>>(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("--> Redis Cache Hit for GetAllCategoriesAsync");
            return cached;
        }

        _logger.LogInformation("--> Redis Cache Miss for GetAllCategoriesAsync, loading from DB");
        var categories = await _categoryRepository.GetAllAsync();
        await _cacheService.SetAsync(cacheKey, categories);
        return categories;
    }
}
