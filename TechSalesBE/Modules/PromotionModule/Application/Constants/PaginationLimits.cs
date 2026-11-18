namespace PromotionModule.Application.Constants;

internal static class PaginationLimits
{
    public const int MinimumPageNumber = 1;
    public const int MaximumPageNumber = 1_000_000;
    public const int MinimumPageSize = 1;
    public const int DefaultPageNumber = MinimumPageNumber;
    public const int DefaultPageSize = 20;
    public const int MaximumPageSize = 100;
}
