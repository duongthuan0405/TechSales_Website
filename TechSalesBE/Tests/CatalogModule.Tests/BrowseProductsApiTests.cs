using System.Net;
using System.Text.Json;
using CatalogModule.Domain.Enums;
using CatalogModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatalogModule.Tests;

public sealed class BrowseProductsApiTests
{
    [Fact]
    public async Task Product_list_and_detail_include_available_and_reserved_stock()
    {
        ProductInventoryFake inventory = new ProductInventoryFake();
        await using ProductTestApp app = await ProductTestApp.CreateAsync(inventory: inventory);
        (Guid active, _) = await SeedAsync(app);
        inventory.Stocks.Add(new CatalogModule.Application.Ports.ProductInventory(active, 7, 3));
        foreach (string route in new List<string> { "/api/products", $"/api/products/{active}" })
        {
            using HttpResponseMessage response = await app.SendAsync(HttpMethod.Get, route);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            JsonElement stock = json.RootElement.GetProperty("data").GetProperty("items")[0].GetProperty("inventory");
            Assert.Equal(7, stock.GetProperty("quantity").GetInt32());
            Assert.Equal(3, stock.GetProperty("reservedQuantity").GetInt32());
        }
        inventory.Stocks.Clear();
        using HttpResponseMessage missingStock = await app.SendAsync(HttpMethod.Get, $"/api/products/{active}");
        using JsonDocument missing = JsonDocument.Parse(await missingStock.Content.ReadAsStringAsync());
        Assert.Equal(0, missing.RootElement.GetProperty("data").GetProperty("items")[0]
            .GetProperty("inventory").GetProperty("quantity").GetInt32());
    }

    [Fact]
    public async Task Catalog_contract_reads_real_products_including_discontinued_without_inventory_dependency()
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        (Guid active, Guid inactive) = await SeedAsync(app);
        var products = await app.WithDatabaseAsync(db =>
            new CatalogModule.Infrastructure.Services.ProductCatalog(db).GetProductsAsync(
                new List<Guid> { active, inactive, Guid.NewGuid() }, CancellationToken.None));
        Assert.Equal(2, products.Count);
        Assert.Equal("Mock brand", products.Single(product => product.Id == active).BrandName);
        Assert.Equal("DISCONTINUED", products.Single(product => product.Id == inactive).Status);
    }

    [Theory]
    [InlineData("/api/products", 1)]
    [InlineData("/api/admin/products", 2)]
    public async Task Brand_filter_combines_with_category_and_filters_before_pagination(string route, int expectedCount)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        await SeedAsync(app);
        Guid brandId = app.References.Brands[0].Id;
        Guid categoryId = app.References.Categories[0].Id;
        (Guid otherBrandId, _) = await app.AddReferencesAsync("Other brand", "Other category");
        await app.WithDatabaseAsync(async db =>
        {
            db.Products.Add(new ProductDbModel
            {
                Id = Guid.NewGuid(), Name = "Other brand product", Description = "Details", Price = 123L,
                Status = ProductStatus.ACTIVE, BrandId = otherBrandId, CategoryId = categoryId,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
            return true;
        });
        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Get,
            $"{route}?brandId={brandId}&categoryId={categoryId}&pageSize=1", Guid.NewGuid(), role: "Staff");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(expectedCount, data.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, data.GetProperty("items").GetArrayLength());
        Assert.Equal(brandId, data.GetProperty("items")[0].GetProperty("brandId").GetGuid());

        using HttpResponseMessage missing = await app.SendAsync(HttpMethod.Get,
            $"{route}?brandId={Guid.NewGuid()}", Guid.NewGuid(), role: "Staff");
        Assert.Equal(HttpStatusCode.OK, missing.StatusCode);
        using JsonDocument empty = JsonDocument.Parse(await missing.Content.ReadAsStringAsync());
        Assert.Equal(0, empty.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());

        using HttpResponseMessage invalid = await app.SendAsync(HttpMethod.Get,
            $"{route}?brandId=invalid", Guid.NewGuid(), role: "Staff");
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [Fact]
    public async Task Products_read_brand_and_category_from_catalog_database()
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync(useDatabaseReferences: true);
        await SeedAsync(app);
        app.References.Brands.Clear();
        app.References.Categories.Clear();
        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Get, "/api/products");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal("Mock brand", data.GetProperty("items")[0].GetProperty("brandName").GetString());
        Assert.Equal("Mock category", data.GetProperty("categories")[0].GetProperty("name").GetString());
        using HttpResponseMessage brands = await app.SendAsync(HttpMethod.Get, "/api/brands");
        Assert.Equal(HttpStatusCode.OK, brands.StatusCode);
    }

    private static async Task<(Guid Active, Guid Inactive)> SeedAsync(ProductTestApp app)
    {
        (Guid brandId, Guid categoryId) = await app.AddReferencesAsync("Mock brand", "Mock category");
        Guid active = Guid.NewGuid();
        Guid inactive = Guid.NewGuid();
        await app.WithDatabaseAsync(async db =>
        {
            db.Products.Add(new ProductDbModel { Id = active, Name = "Laptop active", Description = "Details",
                Price = 123L, Status = ProductStatus.ACTIVE, BrandId = brandId, CategoryId = categoryId, CreatedAt = DateTimeOffset.UtcNow });
            db.Products.Add(new ProductDbModel { Id = inactive, Name = "Laptop inactive", Description = "Details",
                Price = 456L, Status = ProductStatus.DISCONTINUED, BrandId = brandId, CategoryId = categoryId, CreatedAt = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync();
            return true;
        });
        return (active, inactive);
    }

    [Fact]
    public async Task Public_products_use_reference_names_and_only_lists_active_products()
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        (Guid active, Guid inactive) = await SeedAsync(app);
        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Get, "/api/products?status=DISCONTINUED&includeInactive=true");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(1, data.GetProperty("totalCount").GetInt32());
        Assert.Equal(active, data.GetProperty("items")[0].GetProperty("id").GetGuid());
        Assert.Equal("Mock brand", data.GetProperty("items")[0].GetProperty("brandName").GetString());
        Assert.Equal("Mock category", data.GetProperty("categories")[0].GetProperty("name").GetString());
        using HttpResponseMessage hidden = await app.SendAsync(HttpMethod.Get, $"/api/products/{inactive}");
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
    }

    [Theory]
    [InlineData("Staff")]
    [InlineData("Business Admin")]
    [InlineData("Technical Admin")]
    public async Task Authorized_readers_can_see_inactive_products_and_filter_by_keyword(string role)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        await SeedAsync(app);
        using HttpResponseMessage all = await app.SendAsync(HttpMethod.Get, "/api/admin/products", Guid.NewGuid(), role: role);
        Assert.Equal(HttpStatusCode.OK, all.StatusCode);
        using JsonDocument allJson = JsonDocument.Parse(await all.Content.ReadAsStringAsync());
        Assert.Equal(2, allJson.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());
        using HttpResponseMessage filtered = await app.SendAsync(HttpMethod.Get,
            "/api/admin/products?keyword=MOCK&status=DISCONTINUED&pageSize=1", Guid.NewGuid(), role: role);
        using JsonDocument filteredJson = JsonDocument.Parse(await filtered.Content.ReadAsStringAsync());
        Assert.Equal(1, filteredJson.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task Reference_names_and_brand_search_come_from_the_port_without_changing_legacy_rows()
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        await SeedAsync(app);
        app.References.Brands[0] = app.References.Brands[0] with { Name = "External brand" };
        app.References.Categories[0] = app.References.Categories[0] with { Name = "External category" };

        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Get,
            "/api/admin/products?keyword=EXTERNAL", Guid.NewGuid(), role: "Business Admin");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(2, data.GetProperty("totalCount").GetInt32());
        Assert.Equal("External brand", data.GetProperty("items")[0].GetProperty("brandName").GetString());
        Assert.Equal("External category", data.GetProperty("items")[0].GetProperty("categoryName").GetString());
        Assert.Equal("Mock brand", await app.WithDatabaseAsync(db => db.Brands.Select(brand => brand.Name).SingleAsync()));
        Assert.Equal("Mock category", await app.WithDatabaseAsync(db => db.Categorys.Select(category => category.Name).SingleAsync()));
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("Customer", HttpStatusCode.Forbidden)]
    public async Task Management_products_rejects_unauthorized_readers(string? role, HttpStatusCode expected)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        Guid? actor = null;
        if (role != null) { actor = Guid.NewGuid(); }
        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Get, "/api/admin/products", actor, role: role);
        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData("pageNumber=0")]
    [InlineData("pageSize=101")]
    [InlineData("pageNumber=2147483647")]
    public async Task Pagination_is_validated_before_querying(string query)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Get, "/api/products?" + query);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
