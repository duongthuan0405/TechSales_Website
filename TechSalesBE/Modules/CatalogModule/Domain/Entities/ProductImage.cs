using CatalogModule.Domain.Exceptions;
using CatalogModule.Domain.Rules;
using CommonModule.Domain.Exceptions;

namespace CatalogModule.Domain.Entities;

internal sealed class ProductImage
{
    public Guid Id { get; }
    public Guid ProductId { get; }
    public string ImageUrl { get; }
    public bool IsPrimary { get; }
    public DateTimeOffset CreatedAt { get; }

    private ProductImage(Guid productId, string imageUrl, bool isPrimary, DateTimeOffset now)
        : this(Guid.NewGuid(), productId, imageUrl, isPrimary, now.ToUniversalTime())
    {
    }

    private ProductImage(Guid id, Guid productId, string imageUrl, bool isPrimary, DateTimeOffset createdAt)
    {
        Id = id;
        ProductId = productId;
        ImageUrl = imageUrl;
        IsPrimary = isPrimary;
        CreatedAt = createdAt;
    }

    public static ProductImage Restore(Guid id, Guid productId, string imageUrl, bool isPrimary, DateTimeOffset createdAt)
    {
        return new ProductImage(id, productId, imageUrl, isPrimary, createdAt);
    }

    public static ProductImage Create(Guid productId, string imageUrl, bool isPrimary, DateTimeOffset now)
    {
        if (!ProductImageRules.ValidProductId(productId) || !ProductImageRules.ValidUrl(imageUrl))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PRODUCT_IMAGE,
                DomainExceptionMessage.ProductImageInvalid);
        }

        return new ProductImage(productId, imageUrl, isPrimary, now);
    }
}
