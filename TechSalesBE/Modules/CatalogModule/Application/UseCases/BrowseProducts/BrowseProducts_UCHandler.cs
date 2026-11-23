using CatalogModule.Application.Exceptions;
using CatalogModule.Application.Ports;
using CatalogModule.Application.Repositories;
using CatalogModule.Domain.Entities;
using CatalogModule.Domain.Enums;
using CatalogModule.Domain.Rules;
using CatalogModule.Domain.ValueObjects;
using CommonModule.BusinessExceptions;
using MediatR;

namespace CatalogModule.Application.UseCases.BrowseProducts;

internal sealed class BrowseProducts_UCHandler : IRequestHandler<BrowseProducts_UCInput, BrowseProducts_UCOutput>
{
    private readonly IProductRepository _products;
    private readonly IProductReferenceData _references;

    public BrowseProducts_UCHandler(IProductRepository products, IProductReferenceData references)
    {
        _products = products;
        _references = references;
    }

    public async Task<BrowseProducts_UCOutput> Handle(BrowseProducts_UCInput input, CancellationToken cancellationToken)
    {
        ProductStatus? status = null;
        if (!input.IncludeInactive)
        {
            status = ProductStatus.ACTIVE;
        }
        else if (input.Status != null)
        {
            status = ProductRules.ParseStatus(input.Status);
        }

        List<ProductReference> brands = await _references.GetBrandsAsync(cancellationToken);
        List<ProductReference> categories = await _references.GetCategoriesAsync(cancellationToken);
        List<Guid> keywordBrandIds = new List<Guid>();
        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            string keyword = input.Keyword.Trim();
            keywordBrandIds = brands.Where(brand => brand.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .Select(brand => brand.Id).ToList();
        }

        (List<Product> products, int totalCount) = await _products.GetPageAsync(input.PageNumber,
            input.PageSize, input.Keyword, keywordBrandIds, input.CategoryId, status, input.ProductId, input.BrandId, cancellationToken);
        if (input.ProductId.HasValue && products.Count == 0)
        {
            throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.ProductNotFound);
        }

        Dictionary<Guid, string> brandNames = brands.ToDictionary(item => item.Id, item => item.Name);
        Dictionary<Guid, string> categoryNames = categories.ToDictionary(item => item.Id, item => item.Name);
        List<BrowseProductItem> items = products.Select(product => new BrowseProductItem(product.Id,
            product.Name, product.Description, product.Price, product.Status.ToString(), product.BrandId,
            brandNames.GetValueOrDefault(product.BrandId, string.Empty), product.CategoryId,
            categoryNames.GetValueOrDefault(product.CategoryId, string.Empty), product.CreatedAt, product.UpdatedAt,
            product.Images.Select(image => new BrowseProductImage(image.Id, image.ImageUrl, image.IsPrimary)).ToList())).ToList();
        return new BrowseProducts_UCOutput(items, totalCount, input.PageNumber, input.PageSize,
            brands.Select(brand => new ProductLookupItem(brand.Id, brand.Name)).ToList(),
            categories.Select(category => new ProductLookupItem(category.Id, category.Name)).ToList());
    }
}
