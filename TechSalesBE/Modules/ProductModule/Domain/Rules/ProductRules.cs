using ProductModule.Domain.Constants;
using ProductModule.Domain.Enums;
using ProductModule.Domain.Exceptions;
using CommonModule.Domain.Exceptions;

namespace ProductModule.Domain.Rules;

internal static class ProductRules
{
    public static ProductStatus InitialStatus => ProductStatus.ACTIVE;

    public static bool ValidStatus(string? status)
    {
        return string.Equals(status?.Trim(), nameof(ProductStatus.ACTIVE), StringComparison.OrdinalIgnoreCase)
            || string.Equals(status?.Trim(), nameof(ProductStatus.DISCONTINUED), StringComparison.OrdinalIgnoreCase);
    }

    public static void EnsureValidDetails(string name, string description, long? price, Guid brandId, Guid categoryId)
    {
        if (!ValidName(name))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PRODUCT_NAME, DomainExceptionMessage.ProductNameInvalid);
        }
        if (!ValidDescription(description))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PRODUCT_DESCRIPTION, DomainExceptionMessage.ProductDescriptionInvalid);
        }
        if (!ValidPrice(price))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PRODUCT_PRICE, DomainExceptionMessage.ProductPriceInvalid);
        }
        if (!ValidBrandId(brandId))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PRODUCT_BRAND, DomainExceptionMessage.ProductBrandInvalid);
        }
        if (!ValidCategoryId(categoryId))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PRODUCT_CATEGORY, DomainExceptionMessage.ProductCategoryInvalid);
        }
    }

    public static ProductStatus ParseStatus(string status)
    {
        if (!ValidStatus(status))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PRODUCT_STATUS, DomainExceptionMessage.ProductStatusInvalid);
        }

        return Enum.Parse<ProductStatus>(status.Trim(), ignoreCase: true);
    }

    public static bool ValidName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return name.Trim().Length <= ProductLimits.NameMaxLength;
    }

    public static bool ValidDescription(string? description)
    {
        return description != null;
    }

    public static bool ValidPrice(long? price)
    {
        if (!price.HasValue)
        {
            return true;
        }

        return price.Value >= ProductLimits.MinimumPrice;
    }

    public static bool ValidBrandId(Guid brandId)
    {
        return brandId != Guid.Empty;
    }

    public static bool ValidCategoryId(Guid categoryId)
    {
        return categoryId != Guid.Empty;
    }
}
