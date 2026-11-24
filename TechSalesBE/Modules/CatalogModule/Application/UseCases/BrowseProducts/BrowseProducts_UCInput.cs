using MediatR;

namespace CatalogModule.Application.UseCases.BrowseProducts;

internal sealed record BrowseProducts_UCInput(bool IncludeInactive, int PageNumber = 1, int PageSize = 20,
    string? Keyword = null, Guid? CategoryId = null, string? Status = null,
    Guid? ProductId = null, Guid? BrandId = null, long? MinPrice = null, long? MaxPrice = null,
    string SortBy = "newest") : IRequest<BrowseProducts_UCOutput>;
