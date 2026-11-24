namespace CatalogModule.Domain.Rules;

internal static class ProductBrowseRules
{
    public static bool ValidPriceRange(long? minimum, long? maximum)
    {
        return (!minimum.HasValue || minimum.Value >= 0)
            && (!maximum.HasValue || maximum.Value >= 0)
            && (!minimum.HasValue || !maximum.HasValue || minimum.Value <= maximum.Value);
    }

    public static bool ValidSort(string sortBy)
    {
        return sortBy == "newest" || sortBy == "price_asc" || sortBy == "price_desc" || sortBy == "rating";
    }
}
