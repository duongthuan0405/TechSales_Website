namespace CatalogModule.Application.Constants;

internal static class CatalogPagination
{
    public const int DefaultPageSize = 20;
    public const int MaximumPageSize = 100;
    public const int MaximumPageNumber = int.MaxValue / MaximumPageSize;
}
