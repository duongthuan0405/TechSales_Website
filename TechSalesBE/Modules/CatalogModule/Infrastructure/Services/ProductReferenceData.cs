using CatalogModule.Application.Ports;
using CatalogModule.Application.Repositories;
using CatalogModule.Domain.Entities;
using CatalogModule.Domain.ValueObjects;

namespace CatalogModule.Infrastructure.Services;

internal sealed class ProductReferenceData : IProductReferenceData
{
    private readonly IBrandRepository _brands;
    private readonly ICategoryRepository _categories;

    public ProductReferenceData(IBrandRepository brands, ICategoryRepository categories)
    {
        _brands = brands;
        _categories = categories;
    }

    public async Task<List<ProductReference>> GetBrandsAsync(CancellationToken cancellationToken)
    {
        List<Brand> brands = await _brands.GetAllAsync(cancellationToken);
        return brands.Select(brand => new ProductReference(brand.Id, brand.Name)).ToList();
    }

    public async Task<List<ProductReference>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        List<Category> categories = await _categories.GetAllAsync(cancellationToken);
        return categories.Select(category => new ProductReference(category.Id, category.Name)).ToList();
    }
}
