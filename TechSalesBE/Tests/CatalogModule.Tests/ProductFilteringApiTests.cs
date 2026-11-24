using System.Net;
using System.Text.Json;
using CatalogModule.Application.Ports;
using CatalogModule.Domain.Enums;
using CatalogModule.Infrastructure.Persistence.Models;
using Xunit;

namespace CatalogModule.Tests;

public sealed class ProductFilteringApiTests
{
    [Theory]
    [InlineData("price_asc", "Cheap", "Expensive", null)]
    [InlineData("price_desc", "Expensive", "Cheap", null)]
    [InlineData("newest", "Expensive", "Cheap", null)]
    [InlineData("rating", "Cheap", "Expensive", null)]
    [InlineData("price_asc", "Cheap", "Expensive", "Staff")]
    [InlineData("price_desc", "Expensive", "Cheap", "Business Admin")]
    [InlineData("rating", "Cheap", "Expensive", "Technical Admin")]
    [InlineData("newest", "Expensive", "Cheap", "Staff")]
    public async Task CombinedFiltersSortBeforePagingAndCountAllMatches(string sort, string first, string second, string? role)
    {
        ProductRatingsFake ratings = new ProductRatingsFake();
        await using ProductTestApp app = await ProductTestApp.CreateAsync(ratings: ratings);
        var refs = await app.AddReferencesAsync("Brand", "Category");
        var other = await app.AddReferencesAsync("Other", "Other");
        await app.WithDatabaseAsync(async db =>
        {
            void Add(string name, long? price, Guid brand, Guid category, ProductStatus status, int age, double rating)
            {
                Guid id = Guid.NewGuid();
                db.Products.Add(new ProductDbModel { Id = id, Name = name, Description = "Details", Price = price,
                    BrandId = brand, CategoryId = category, Status = status, CreatedAt = DateTimeOffset.UtcNow.AddDays(-age) });
                ratings.Ratings.Add(new ProductRating(id, rating, 2));
            }
            Add("Cheap", 100, refs.BrandId, refs.CategoryId, ProductStatus.ACTIVE, 2, 5);
            Add("Expensive", 200, refs.BrandId, refs.CategoryId, ProductStatus.ACTIVE, 1, 4);
            Add("Below range", 99, refs.BrandId, refs.CategoryId, ProductStatus.ACTIVE, 0, 5);
            Add("Above range", 201, refs.BrandId, refs.CategoryId, ProductStatus.ACTIVE, 0, 5);
            Add("No price", null, refs.BrandId, refs.CategoryId, ProductStatus.ACTIVE, 0, 5);
            Add("Inactive", 150, refs.BrandId, refs.CategoryId, ProductStatus.DISCONTINUED, 0, 5);
            Add("Wrong brand", 150, other.BrandId, refs.CategoryId, ProductStatus.ACTIVE, 0, 5);
            Add("Wrong category", 150, refs.BrandId, other.CategoryId, ProductStatus.ACTIVE, 0, 5);
            await db.SaveChangesAsync();
            return true;
        });
        for (int page = 1; page <= 3; page++)
        {
            string route = role == null ? "/api/products" : "/api/admin/products";
            using var response = await app.SendAsync(HttpMethod.Get,
                $"{route}?brandId={refs.BrandId}&categoryId={refs.CategoryId}&minPrice=100&maxPrice=200&sortBy={sort}&pageSize=1&pageNumber={page}&status=ACTIVE",
                role == null ? null : Guid.NewGuid(), role: role);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var data = json.RootElement.GetProperty("data");
            Assert.Equal(2, data.GetProperty("totalCount").GetInt32());
            var items = data.GetProperty("items");
            if (page == 3)
            {
                Assert.Equal(0, items.GetArrayLength());
            }
            else
            {
                Assert.Equal(page == 1 ? first : second, items[0].GetProperty("name").GetString());
                Assert.Equal(page == 1 && sort != "price_desc" && sort != "newest" ? 5 : page == 2 && (sort == "price_desc" || sort == "newest") ? 5 : 4,
                    items[0].GetProperty("rating").GetDouble());
            }
        }
    }

    [Theory]
    [InlineData("minPrice=-1")]
    [InlineData("maxPrice=-1")]
    [InlineData("minPrice=200&maxPrice=100")]
    [InlineData("minPrice=1.5")]
    [InlineData("sortBy=unknown")]
    public async Task RejectsInvalidPricesAndSort(string query)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        using var response = await app.SendAsync(HttpMethod.Get, "/api/products?" + query);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
