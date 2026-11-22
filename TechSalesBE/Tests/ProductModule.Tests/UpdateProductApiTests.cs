using System.Net;
using System.Text.Json;
using ProductModule.Domain.Enums;
using ProductModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ProductModule.Tests;

public sealed class UpdateProductApiTests
{
    private static async Task<Guid> CreateProductAsync(ProductTestApp app)
    {
        using MultipartFormDataContent form = await ProductImageApiTests.CreateFormAsync(app, 2);
        using HttpResponseMessage response = await app.SendMultipartAsync("/api/admin/products/with-images", form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("data").GetProperty("id").GetGuid();
    }

    private static async Task<Dictionary<string, object?>> RequestAsync(ProductTestApp app, Guid id)
    {
        ProductDbModel row = await app.WithDatabaseAsync(db => db.Products.AsNoTracking().SingleAsync(product => product.Id == id));
        return new Dictionary<string, object?>
        {
            ["name"] = "  Tên sản phẩm mới  ",
            ["description"] = "Mô tả mới",
            ["price"] = 200000L,
            ["brandId"] = row.BrandId,
            ["categoryId"] = row.CategoryId,
            ["status"] = "DISCONTINUED"
        };
    }

    [Theory]
    [InlineData("Business Admin")]
    [InlineData("Technical Admin")]
    public async Task Administrators_can_edit_and_switch_status_both_ways_without_losing_images(string role)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        Guid id = await CreateProductAsync(app);
        DateTimeOffset createdAt = await app.WithDatabaseAsync(db => db.Products.Select(row => row.CreatedAt).SingleAsync());
        List<Guid> imageIds = await app.WithDatabaseAsync(db => db.ProductImages.OrderBy(row => row.Id).Select(row => row.Id).ToListAsync());
        Dictionary<string, object?> request = await RequestAsync(app, id);
        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Put, $"/api/admin/products/{id}",
            Guid.NewGuid(), request, role);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(id, data.GetProperty("id").GetGuid());
        Assert.Equal("Tên sản phẩm mới", data.GetProperty("name").GetString());
        Assert.Equal("Mô tả mới", data.GetProperty("description").GetString());
        Assert.Equal("DISCONTINUED", data.GetProperty("status").GetString());
        Assert.Equal(200000L, data.GetProperty("price").GetInt64());
        Assert.Equal(2, data.GetProperty("images").GetArrayLength());

        request["status"] = "active";
        using HttpResponseMessage reactivate = await app.SendAsync(HttpMethod.Put, $"/api/admin/products/{id}",
            Guid.NewGuid(), request, role);
        Assert.Equal(HttpStatusCode.OK, reactivate.StatusCode);
        ProductDbModel stored = await app.WithDatabaseAsync(db => db.Products.AsNoTracking().SingleAsync());
        Assert.Equal(ProductStatus.ACTIVE, stored.Status);
        Assert.Equal(createdAt, stored.CreatedAt);
        Assert.NotNull(stored.UpdatedAt);
        Assert.Equal(imageIds, await app.WithDatabaseAsync(db => db.ProductImages.OrderBy(row => row.Id).Select(row => row.Id).ToListAsync()));
        Assert.Equal(2, app.ImageFileCount);
        Assert.Equal(1, await app.WithDatabaseAsync(db => db.Products.CountAsync()));
        Assert.Equal(1, await app.WithDatabaseAsync(db => db.ProductImages.CountAsync(image => image.IsPrimary == true)));
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("Staff", HttpStatusCode.Forbidden)]
    [InlineData("Customer", HttpStatusCode.Forbidden)]
    public async Task Only_administrators_can_edit(string? role, HttpStatusCode expected)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        Guid id = await CreateProductAsync(app);
        Dictionary<string, object?> request = await RequestAsync(app, id);
        Guid? actor = null;
        if (role != null)
        {
            actor = Guid.NewGuid();
        }

        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Put, $"/api/admin/products/{id}", actor, request, role);
        Assert.Equal(expected, response.StatusCode);
        Assert.Equal(ProductStatus.ACTIVE, await app.WithDatabaseAsync(db => db.Products.Select(row => row.Status).SingleAsync()));
    }

    [Theory]
    [InlineData("name", "")]
    [InlineData("description", null)]
    [InlineData("price", -1L)]
    [InlineData("status", "")]
    [InlineData("status", null)]
    [InlineData("status", "INVALID")]
    [InlineData("status", "0")]
    [InlineData("brandId", "00000000-0000-0000-0000-000000000000")]
    [InlineData("categoryId", "00000000-0000-0000-0000-000000000000")]
    public async Task Invalid_edit_is_rejected_without_changing_existing_data(string field, object? value)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        Guid id = await CreateProductAsync(app);
        Dictionary<string, object?> request = await RequestAsync(app, id);
        request[field] = value;
        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Put, $"/api/admin/products/{id}",
            Guid.NewGuid(), request, "Business Admin");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ProductDbModel stored = await app.WithDatabaseAsync(db => db.Products.AsNoTracking().SingleAsync());
        Assert.Equal("Laptop", stored.Name);
        Assert.Equal(ProductStatus.ACTIVE, stored.Status);
        Assert.Null(stored.UpdatedAt);
        Assert.Equal(2, app.ImageFileCount);
    }

    [Theory]
    [InlineData("product")]
    [InlineData("brandId")]
    [InlineData("categoryId")]
    public async Task Missing_product_or_reference_does_not_modify_data(string missing)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        Guid id = await CreateProductAsync(app);
        Dictionary<string, object?> request = await RequestAsync(app, id);
        Guid targetId = id;
        if (missing == "product")
        {
            targetId = Guid.NewGuid();
        }
        else
        {
            request[missing] = Guid.NewGuid();
        }

        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Put, $"/api/admin/products/{targetId}",
            Guid.NewGuid(), request, "Business Admin");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(ProductStatus.ACTIVE, await app.WithDatabaseAsync(db => db.Products.Select(row => row.Status).SingleAsync()));
    }

    [Theory]
    [InlineData(false, HttpStatusCode.InternalServerError)]
    [InlineData(true, HttpStatusCode.Conflict)]
    public async Task Save_failure_preserves_metadata_and_images(bool concurrentDeletion, HttpStatusCode expected)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        Guid id = await CreateProductAsync(app);
        Dictionary<string, object?> request = await RequestAsync(app, id);
        if (concurrentDeletion)
        {
            app.SaveFailure.Failure = new DbUpdateConcurrencyException("concurrent removal");
        }
        else
        {
            app.SaveFailure.FailWrites = true;
        }

        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Put, $"/api/admin/products/{id}",
            Guid.NewGuid(), request, "Business Admin");
        Assert.Equal(expected, response.StatusCode);
        ProductDbModel stored = await app.WithDatabaseAsync(db => db.Products.AsNoTracking().SingleAsync());
        Assert.Equal("Laptop", stored.Name);
        Assert.Equal(ProductStatus.ACTIVE, stored.Status);
        Assert.Null(stored.UpdatedAt);
        Assert.Equal(2, app.ImageFileCount);
        Assert.Equal(2, await app.WithDatabaseAsync(db => db.ProductImages.CountAsync()));
    }

    [Fact]
    public async Task References_can_change_and_price_can_be_cleared()
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        Guid id = await CreateProductAsync(app);
        Dictionary<string, object?> request = await RequestAsync(app, id);
        (Guid newBrand, Guid newCategory) = await app.AddReferencesAsync("New brand", "New category");
        request["brandId"] = newBrand;
        request["categoryId"] = newCategory;
        request["price"] = null;
        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Put, $"/api/admin/products/{id}",
            Guid.NewGuid(), request, "Business Admin");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ProductDbModel stored = await app.WithDatabaseAsync(db => db.Products.AsNoTracking().SingleAsync());
        Assert.Equal(newBrand, stored.BrandId);
        Assert.Equal(newCategory, stored.CategoryId);
        Assert.Null(stored.Price);
        Assert.Equal(2, await app.WithDatabaseAsync(db => db.Brands.CountAsync()));
        Assert.Equal(2, await app.WithDatabaseAsync(db => db.Categorys.CountAsync()));
    }
}
