using System.Net;
using System.Text.Json;
using CatalogModule.Infrastructure.Persistence.Models;
using Xunit;

namespace CatalogModule.Tests;

public sealed class ReferencePaginationApiTests
{
    private static Task<bool> SeedAsync(ProductTestApp app)
    {
        return app.WithDatabaseAsync(async db =>
        {
            foreach (string name in new List<string> { "Alpha 0", "Alpha 1", "Alpha 2", "Alpha 3", "Alpha 4", "Beta 0", "Beta 1", "Other" })
            {
                db.Brands.Add(new BrandDbModel { Id = Guid.NewGuid(), Name = name });
                db.Categorys.Add(new CategoryDbModel { Id = Guid.NewGuid(), Name = name, CreatedAat = DateTimeOffset.UtcNow });
            }
            await db.SaveChangesAsync();
            return true;
        });
    }

    [Theory]
    [InlineData("brands", "Business Admin")]
    [InlineData("categories", "Technical Admin")]
    public async Task Admin_filters_before_paging_and_public_still_returns_the_complete_list(string resource, string role)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        await SeedAsync(app);
        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Get,
            $"/api/admin/{resource}?pageNumber=2&pageSize=2&keyword=%20aLpHa%20", Guid.NewGuid(), role: role);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(5, data.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, data.GetProperty("pageNumber").GetInt32());
        Assert.Equal(2, data.GetProperty("pageSize").GetInt32());
        Assert.Equal("Alpha 2", data.GetProperty("items")[0].GetProperty("name").GetString());
        Assert.Equal("Alpha 3", data.GetProperty("items")[1].GetProperty("name").GetString());

        using HttpResponseMessage beyond = await app.SendAsync(HttpMethod.Get,
            $"/api/admin/{resource}?pageNumber=10&pageSize=2&keyword=Alpha", Guid.NewGuid(), role: role);
        using JsonDocument empty = JsonDocument.Parse(await beyond.Content.ReadAsStringAsync());
        Assert.Equal(5, empty.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());
        Assert.Equal(0, empty.RootElement.GetProperty("data").GetProperty("items").GetArrayLength());

        using HttpResponseMessage publicResponse = await app.SendAsync(HttpMethod.Get, $"/api/{resource}?pageSize=1&keyword=Alpha");
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);
        using JsonDocument publicJson = JsonDocument.Parse(await publicResponse.Content.ReadAsStringAsync());
        Assert.Equal(8, publicJson.RootElement.GetProperty("data").GetArrayLength());
    }

    [Theory]
    [InlineData("brands")]
    [InlineData("categories")]
    public async Task Admin_requires_admin_role_and_validates_pagination(string resource)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        using HttpResponseMessage anonymous = await app.SendAsync(HttpMethod.Get, $"/api/admin/{resource}");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        foreach (string role in new List<string> { "Customer", "Staff" })
        {
            using HttpResponseMessage forbidden = await app.SendAsync(HttpMethod.Get, $"/api/admin/{resource}", Guid.NewGuid(), role: role);
            Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        }
        foreach (string query in new List<string> { "pageNumber=0", "pageSize=0", "pageSize=101", "pageNumber=2147483647", "pageSize=abc" })
        {
            using HttpResponseMessage invalid = await app.SendAsync(HttpMethod.Get,
                $"/api/admin/{resource}?{query}", Guid.NewGuid(), role: "Business Admin");
            Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        }
        using HttpResponseMessage valid = await app.SendAsync(HttpMethod.Get, $"/api/admin/{resource}", Guid.NewGuid(), role: "Business Admin");
        Assert.Equal(HttpStatusCode.OK, valid.StatusCode);
        using JsonDocument json = JsonDocument.Parse(await valid.Content.ReadAsStringAsync());
        Assert.Equal(20, json.RootElement.GetProperty("data").GetProperty("pageSize").GetInt32());
        Assert.Equal(0, json.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());
    }
}
