using ProductModule.Domain.Constants;
using ProductModule.Domain.Entities;
using ProductModule.Domain.Enums;
using ProductModule.Domain.Exceptions;
using CommonModule.Domain.Exceptions;
using Xunit;

namespace ProductModule.Tests;

public sealed class CreateProductDomainTests
{
    [Fact]
    public void Invalid_status_cannot_partially_change_the_product()
    {
        Product product = Product.Create("Laptop", "Mô tả", 100L,
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);

        DomainException exception = Assert.Throws<DomainException>(() =>
            product.Update("Changed", "Changed", 200L, product.BrandId, product.CategoryId,
                "INVALID", DateTimeOffset.UtcNow));

        Assert.Equal(DomainExceptionCode.INVALID_PRODUCT_STATUS, exception.Code);
        Assert.Equal("Laptop", product.Name);
        Assert.Equal(100L, product.Price);
        Assert.Equal(ProductStatus.ACTIVE, product.Status);
        Assert.Null(product.UpdatedAt);
    }

    [Fact]
    public void Clearing_the_returned_image_list_cannot_remove_product_images()
    {
        Product product = Product.Create("Laptop", "Mô tả", 100L, Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow, new List<string> { "/product-images/example.png" });

        product.Images.Clear();

        Assert.Single(product.Images);
        Assert.True(product.Images[0].IsPrimary);
    }

    [Fact]
    public void Create_sets_identity_initial_state_and_utc_time()
    {
        Guid brandId = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();
        DateTimeOffset now = new DateTimeOffset(2026, 11, 23, 9, 0, 0, TimeSpan.FromHours(7));

        Product product = Product.Create("  Laptop  ", "Mô tả sản phẩm", 15_000_000L,
            brandId, categoryId, now);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("Laptop", product.Name);
        Assert.Equal("Mô tả sản phẩm", product.Description);
        Assert.Equal(15_000_000L, product.Price);
        Assert.Equal(brandId, product.BrandId);
        Assert.Equal(categoryId, product.CategoryId);
        Assert.Equal(ProductStatus.ACTIVE, product.Status);
        Assert.Equal(now.ToUniversalTime(), product.CreatedAt);
        Assert.Equal(TimeSpan.Zero, product.CreatedAt.Offset);
        Assert.Null(product.UpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0L)]
    [InlineData(long.MaxValue)]
    public void Create_accepts_nullable_and_non_negative_vnd_prices(long? price)
    {
        Product product = Product.Create("Laptop", string.Empty, price,
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Equal(price, product.Price);
        Assert.Equal(string.Empty, product.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_missing_name_without_a_validator(string? name)
    {
        DomainException exception = Assert.Throws<DomainException>(() =>
            Product.Create(name!, "Mô tả", 1L, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow));

        Assert.Equal(DomainExceptionCode.INVALID_PRODUCT_NAME, exception.Code);
    }

    [Fact]
    public void Create_checks_name_length_after_trimming()
    {
        string maximumName = new string('a', ProductLimits.NameMaxLength);
        Product product = Product.Create(" " + maximumName + " ", "Mô tả", 1L,
            Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
        Assert.Equal(maximumName, product.Name);

        DomainException exception = Assert.Throws<DomainException>(() =>
            Product.Create(maximumName + "a", "Mô tả", 1L,
                Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow));
        Assert.Equal(DomainExceptionCode.INVALID_PRODUCT_NAME, exception.Code);
    }

    [Fact]
    public void Create_rejects_negative_price()
    {
        DomainException exception = Assert.Throws<DomainException>(() =>
            Product.Create("Laptop", "Mô tả", -1L,
                Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow));

        Assert.Equal(DomainExceptionCode.INVALID_PRODUCT_PRICE, exception.Code);
    }

    [Fact]
    public void Create_rejects_null_description()
    {
        DomainException exception = Assert.Throws<DomainException>(() =>
            Product.Create("Laptop", null!, 1L,
                Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow));

        Assert.Equal(DomainExceptionCode.INVALID_PRODUCT_DESCRIPTION, exception.Code);
    }

    [Fact]
    public void Create_requires_brand_identity()
    {
        DomainException exception = Assert.Throws<DomainException>(() =>
            Product.Create("Laptop", "Mô tả", 1L,
                Guid.Empty, Guid.NewGuid(), DateTimeOffset.UtcNow));

        Assert.Equal(DomainExceptionCode.INVALID_PRODUCT_BRAND, exception.Code);
    }

    [Fact]
    public void Create_requires_category_identity()
    {
        DomainException exception = Assert.Throws<DomainException>(() =>
            Product.Create("Laptop", "Mô tả", 1L,
                Guid.NewGuid(), Guid.Empty, DateTimeOffset.UtcNow));

        Assert.Equal(DomainExceptionCode.INVALID_PRODUCT_CATEGORY, exception.Code);
    }
}
