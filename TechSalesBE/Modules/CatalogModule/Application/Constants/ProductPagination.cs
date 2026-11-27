namespace CatalogModule.Application.Constants;

internal static class ProductPagination
{
    public const int MaximumPageSize = 100;
    public const int MaximumPageNumber = int.MaxValue / MaximumPageSize;
}
