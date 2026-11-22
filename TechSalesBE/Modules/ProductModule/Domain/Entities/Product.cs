using ProductModule.Domain.Enums;
using ProductModule.Domain.Exceptions;
using ProductModule.Domain.Rules;
using CommonModule.Domain.Exceptions;

namespace ProductModule.Domain.Entities;

internal sealed class Product
{
    private readonly List<ProductImage> _images = new List<ProductImage>();

    public Guid Id { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public long? Price { get; private set; }
    public ProductStatus Status { get; private set; }
    public Guid BrandId { get; private set; }
    public Guid CategoryId { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public List<ProductImage> Images => _images.ToList();

    private Product(Guid id, string name, string description, long? price,
        ProductStatus status, Guid brandId, Guid categoryId, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        Status = status;
        BrandId = brandId;
        CategoryId = categoryId;
        CreatedAt = createdAt;
        UpdatedAt = null;
    }

    public static Product Restore(Guid id, string name, string description, long? price,
        ProductStatus status, Guid brandId, Guid categoryId, DateTimeOffset createdAt,
        DateTimeOffset? updatedAt, List<ProductImage> images)
    {
        Product product = new Product(id, name, description, price, status, brandId, categoryId, createdAt);
        product.UpdatedAt = updatedAt;
        product._images.AddRange(images);
        return product;
    }

    public void Update(string name, string description, long? price, Guid brandId, Guid categoryId,
        string status, DateTimeOffset now)
    {
        ProductRules.EnsureValidDetails(name, description, price, brandId, categoryId);
        ProductStatus parsedStatus = ProductRules.ParseStatus(status);
        Name = name.Trim();
        Description = description;
        Price = price;
        BrandId = brandId;
        CategoryId = categoryId;
        Status = parsedStatus;
        UpdatedAt = now.ToUniversalTime();
    }

    public static Product Create(string name, string description, long? price,
        Guid brandId, Guid categoryId, DateTimeOffset now,
        List<string>? imageUrls = null, int? primaryImageIndex = null)
    {
        ProductRules.EnsureValidDetails(name, description, price, brandId, categoryId);

        List<string> urls = imageUrls ?? new List<string>();
        if (!ProductImageRules.ValidCount(urls.Count))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PRODUCT_IMAGES,
                DomainExceptionMessage.ProductImagesInvalid);
        }

        if (!ProductImageRules.ValidPrimaryIndex(urls.Count, primaryImageIndex))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PRODUCT_IMAGES,
                DomainExceptionMessage.PrimaryImageInvalid);
        }

        Product product = new Product(Guid.NewGuid(), name.Trim(), description, price,
            ProductRules.InitialStatus, brandId, categoryId, now.ToUniversalTime());
        for (int index = 0; index < urls.Count; index++)
        {
            product._images.Add(ProductImage.Create(product.Id, urls[index],
                ProductImageRules.IsPrimary(index, primaryImageIndex), now));
        }

        return product;
    }
}
