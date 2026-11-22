namespace ProductModule.Application.UseCases.BrowseProducts;

public sealed record BrowseProducts_UCOutput(List<BrowseProductItem> Items, int TotalCount,
    int PageNumber, int PageSize, List<ProductLookupItem> Brands, List<ProductLookupItem> Categories);

public sealed record ProductLookupItem(Guid Id, string Name);

public sealed record BrowseProductItem(Guid Id, string Name, string Description, long? Price, string Status,
    Guid BrandId, string BrandName, Guid CategoryId, string CategoryName, DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt, List<BrowseProductImage> Images);

public sealed record BrowseProductImage(Guid Id, string ImageUrl, bool IsPrimary);
