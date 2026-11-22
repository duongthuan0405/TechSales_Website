using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using ProductModule.Domain.Constants;
using ProductModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ProductModule.Tests;

public sealed class ProductImageApiTests
{
    private const string Endpoint = "/api/admin/products/with-images";
    internal static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+j2ioAAAAASUVORK5CYII=");

    internal static async Task<MultipartFormDataContent> CreateFormAsync(ProductTestApp app,
        int count, string contentType = "image/png", byte[]? bytes = null)
    {
        (Guid brandId, Guid categoryId) = await app.AddReferencesAsync(Guid.NewGuid().ToString(), "Laptop");

        MultipartFormDataContent form = new MultipartFormDataContent
        {
            { new StringContent("Laptop"), "Name" },
            { new StringContent("Mô tả"), "Description" },
            { new StringContent("100000"), "Price" },
            { new StringContent(brandId.ToString()), "BrandId" },
            { new StringContent(categoryId.ToString()), "CategoryId" }
        };
        for (int index = 0; index < count; index++)
        {
            AddFile(form, bytes ?? Png, contentType, "../../untrusted.png");
        }

        return form;
    }

    internal static void AddFile(MultipartFormDataContent form, byte[] bytes, string contentType, string fileName)
    {
        ByteArrayContent file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(file, "Images", fileName);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData(1, 1)]
    public async Task Multiple_files_are_saved_and_exactly_one_is_primary(int? primaryIndex, int expected)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        using MultipartFormDataContent form = await CreateFormAsync(app, 3);
        if (primaryIndex.HasValue)
        {
            form.Add(new StringContent(primaryIndex.Value.ToString()), "PrimaryImageIndex");
        }

        using HttpResponseMessage response = await app.SendMultipartAsync(Endpoint, form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement data = json.RootElement.GetProperty("data");
        List<JsonElement> images = data.GetProperty("images").EnumerateArray().ToList();
        Assert.Equal(3, images.Count);
        Assert.Single(images, image => image.GetProperty("isPrimary").GetBoolean());
        Assert.True(images[expected].GetProperty("isPrimary").GetBoolean());
        Assert.Equal(3, app.ImageFileCount);
        Assert.Equal(3, await app.WithDatabaseAsync(db => db.ProductImages.CountAsync()));
        Guid productId = data.GetProperty("id").GetGuid();
        Assert.True(await app.WithDatabaseAsync(db => db.ProductImages.AllAsync(image => image.ProductId == productId)));
        foreach (JsonElement image in images)
        {
            string url = image.GetProperty("imageUrl").GetString()!;
            Assert.Matches("^/product-images/[a-f0-9]{32}\\.png$", url);
            using HttpResponseMessage download = await app.SendAsync(HttpMethod.Get, url);
            Assert.Equal(HttpStatusCode.OK, download.StatusCode);
            Assert.Equal(Png, await download.Content.ReadAsByteArrayAsync());
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public async Task Invalid_primary_index_does_not_write_files_or_rows(int primaryIndex)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        using MultipartFormDataContent form = await CreateFormAsync(app, 2);
        form.Add(new StringContent(primaryIndex.ToString()), "PrimaryImageIndex");
        using HttpResponseMessage response = await app.SendMultipartAsync(Endpoint, form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, app.ImageFileCount);
        Assert.Equal(0, await app.WithDatabaseAsync(db => db.Products.CountAsync()));
    }

    [Fact]
    public async Task Too_many_images_are_rejected_before_uploading()
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        using MultipartFormDataContent form = await CreateFormAsync(app, ProductLimits.MaximumImages + 1);
        using HttpResponseMessage response = await app.SendMultipartAsync(Endpoint, form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, app.ImageFileCount);
    }

    [Theory]
    [InlineData("text/plain")]
    [InlineData("image/svg+xml")]
    public async Task Unsupported_content_type_is_rejected(string contentType)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        using MultipartFormDataContent form = await CreateFormAsync(app, 1, contentType);
        using HttpResponseMessage response = await app.SendMultipartAsync(Endpoint, form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, app.ImageFileCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Failed_upload_or_database_save_removes_already_uploaded_files(bool databaseFailure)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        using MultipartFormDataContent form = await CreateFormAsync(app, 1);
        if (databaseFailure)
        {
            app.SaveFailure.FailWrites = true;
        }
        else
        {
            AddFile(form, new byte[] { 1, 2, 3 }, "image/png", "fake.png");
        }

        using HttpResponseMessage response = await app.SendMultipartAsync(Endpoint, form);
        Assert.Equal(databaseFailure ? HttpStatusCode.InternalServerError : HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, app.ImageFileCount);
        Assert.Equal(0, await app.WithDatabaseAsync(db => db.Products.CountAsync()));
        Assert.Equal(0, await app.WithDatabaseAsync(db => db.ProductImages.CountAsync()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5242881)]
    public async Task Empty_or_oversized_images_are_rejected(int size)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        using MultipartFormDataContent form = await CreateFormAsync(app, 1, bytes: new byte[size]);
        using HttpResponseMessage response = await app.SendMultipartAsync(Endpoint, form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, app.ImageFileCount);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("Staff", HttpStatusCode.Forbidden)]
    public async Task Upload_requires_an_administrator(string? role, HttpStatusCode expected)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        using MultipartFormDataContent form = await CreateFormAsync(app, 1);
        using HttpResponseMessage response = await app.SendMultipartAsync(Endpoint, form, role);
        Assert.Equal(expected, response.StatusCode);
        Assert.Equal(0, app.ImageFileCount);
    }
}
