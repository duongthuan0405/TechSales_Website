using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TechSalesManagement.Application.Exceptions;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Application.Services.Params;
using TechSalesManagement.Common;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Implementations;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, ICacheService cacheService, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<List<Product>> SearchProductsAsync(SearchProductParams parameters)
    {
        // Compute a unique key based on search parameters
        var keyword = parameters.Keyword?.Trim();
        var cacheKey = $"products:search:{keyword ?? ""}_{string.Join("-", parameters.CategoryIds ?? new List<Guid>())}_{parameters.SortOrder}";
        
        var cached = await _cacheService.GetAsync<List<Product>>(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("--> Redis Cache Hit for Product Search");
            return cached;
        }

        _logger.LogInformation("--> Redis Cache Miss for Product Search, querying DB");
        var result = await _productRepository.GetProductsAsync(keyword, parameters.CategoryIds, parameters.SortOrder, TechSalesManagement.Domain.Enums.ProductStatus.ACTIVE);
        
        // Cache search results using environment default
        await _cacheService.SetAsync(cacheKey, result);
        return result;
    }

    public async Task<Product> GetProductDetailsAsync(GetProductDetailsParams parameters)
    {
        var product = await _productRepository.GetByIdAsync(parameters.ProductId);
        if (product == null)
        {
            throw new NotFoundException(MessageConstants.MSG25);
        }
        return product;
    }
}
