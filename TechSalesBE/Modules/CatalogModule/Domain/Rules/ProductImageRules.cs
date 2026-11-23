using CatalogModule.Domain.Constants;

namespace CatalogModule.Domain.Rules;

internal static class ProductImageRules
{
    public static bool ValidRetainedImages(List<Guid> retainedIds, List<Guid> existingIds)
    {
        return retainedIds.Distinct().Count() == retainedIds.Count
            && retainedIds.All(existingIds.Contains);
    }

    public static bool ValidProductId(Guid productId)
    {
        return productId != Guid.Empty;
    }

    public static bool ValidCount(int count)
    {
        return count >= 0 && count <= ProductLimits.MaximumImages;
    }

    public static bool ValidPrimaryIndex(int count, int? primaryIndex)
    {
        if (!primaryIndex.HasValue)
        {
            return true;
        }

        return primaryIndex.Value >= 0 && primaryIndex.Value < count;
    }

    public static bool IsPrimary(int index, int? primaryIndex)
    {
        return index == primaryIndex.GetValueOrDefault();
    }

    public static bool ValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || url.Length > ProductLimits.ImageUrlMaxLength)
        {
            return false;
        }

        // Storage returns an application-relative URL, never a local filesystem path.
        return url.StartsWith("/product-images/", StringComparison.Ordinal)
            && !url.Contains("..", StringComparison.Ordinal)
            && !url.Contains('\\');
    }
}
