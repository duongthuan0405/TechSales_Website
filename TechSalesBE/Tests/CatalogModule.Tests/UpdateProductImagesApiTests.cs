using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatalogModule.Tests;

public sealed class UpdateProductImagesApiTests
{
    private static async Task<(Guid Id, Guid ImageId)> CreateAsync(ProductTestApp app)
    {
        using MultipartFormDataContent form = await ProductImageApiTests.CreateFormAsync(app, 2);
        using HttpResponseMessage response = await app.SendMultipartAsync("/api/admin/products/with-images", form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement data = json.RootElement.GetProperty("data");
        return (data.GetProperty("id").GetGuid(), data.GetProperty("images")[0].GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task Retains_selected_image_adds_multiple_files_changes_primary_and_removes_old_file()
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        (Guid id, Guid imageId) = await CreateAsync(app);
        using MultipartFormDataContent form = new MultipartFormDataContent
        {
            { new StringContent(imageId.ToString()), "RetainedImageIds" },
            { new StringContent("2"), "PrimaryImageIndex" }
        };
        ProductImageApiTests.AddFile(form, ProductImageApiTests.Png, "image/png", "a.png");
        ProductImageApiTests.AddFile(form, ProductImageApiTests.Png, "image/png", "b.png");
        using HttpResponseMessage response = await app.SendMultipartAsync($"/api/admin/products/{id}/images", form, method: HttpMethod.Put);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        List<JsonElement> images = json.RootElement.GetProperty("data").GetProperty("images").EnumerateArray().ToList();
        Assert.Equal(3, images.Count);
        Assert.Equal(imageId, images[0].GetProperty("id").GetGuid());
        Assert.True(images[2].GetProperty("isPrimary").GetBoolean());
        Assert.Single(images, image => image.GetProperty("isPrimary").GetBoolean());
        Assert.Equal(3, app.ImageFileCount);
        Assert.Equal(3, await app.WithDatabaseAsync(db => db.ProductImages.CountAsync()));

        using MultipartFormDataContent empty = new MultipartFormDataContent
        {
            { new StringContent(""), "PrimaryImageIndex" }
        };
        using HttpResponseMessage cleared = await app.SendMultipartAsync($"/api/admin/products/{id}/images", empty, method: HttpMethod.Put);
        Assert.Equal(HttpStatusCode.OK, cleared.StatusCode);
        Assert.Equal(0, app.ImageFileCount);
        Assert.Equal(0, await app.WithDatabaseAsync(db => db.ProductImages.CountAsync()));
    }

    [Theory]
    [InlineData("foreign")]
    [InlineData("duplicate")]
    [InlineData("primary")]
    [InlineData("count")]
    [InlineData("file")]
    [InlineData("storage")]
    public async Task Invalid_update_or_failed_commit_preserves_original_images(string failure)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        (Guid id, Guid imageId) = await CreateAsync(app);
        using MultipartFormDataContent form = new MultipartFormDataContent
        {
            { new StringContent((failure == "foreign" ? Guid.NewGuid() : imageId).ToString()), "RetainedImageIds" }
        };
        if (failure == "duplicate")
        {
            form.Add(new StringContent(imageId.ToString()), "RetainedImageIds");
        }
        if (failure == "primary")
        {
            form.Add(new StringContent("99"), "PrimaryImageIndex");
        }
        int count = failure == "count" ? 10 : 1;
        for (int index = 0; index < count; index++)
        {
            ProductImageApiTests.AddFile(form, failure == "file" ? new byte[] { 1, 2 } : ProductImageApiTests.Png, "image/png", "a.png");
        }
        app.SaveFailure.FailWrites = failure == "storage";
        using HttpResponseMessage response = await app.SendMultipartAsync($"/api/admin/products/{id}/images", form, method: HttpMethod.Put);
        HttpStatusCode expected = HttpStatusCode.BadRequest;
        if (failure == "storage")
        {
            expected = HttpStatusCode.InternalServerError;
        }
        else if (failure == "foreign" || failure == "duplicate")
        {
            expected = HttpStatusCode.UnprocessableEntity;
        }
        Assert.Equal(expected, response.StatusCode);
        Assert.Equal(2, app.ImageFileCount);
        Assert.Equal(2, await app.WithDatabaseAsync(db => db.ProductImages.CountAsync()));
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("Staff", HttpStatusCode.Forbidden)]
    [InlineData("Business Admin", HttpStatusCode.NotFound)]
    [InlineData("Technical Admin", HttpStatusCode.NotFound)]
    public async Task Requires_admin_and_existing_product(string? role, HttpStatusCode expected)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        using MultipartFormDataContent form = new MultipartFormDataContent
        {
            { new StringContent(""), "PrimaryImageIndex" }
        };
        using HttpResponseMessage response = await app.SendMultipartAsync($"/api/admin/products/{Guid.NewGuid()}/images", form, role, HttpMethod.Put);
        Assert.Equal(expected, response.StatusCode);
    }
}
