using System.Net;
using System.Text.Json;
using ProductModule.Domain.Enums;
using ProductModule.Infrastructure.Persistence;
using ProductModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace ProductModule.Tests;

public sealed class CreateProductApiTests
{
    private const string Endpoint = "/api/admin/products";

    [Fact]
    public void Creation_feature_does_not_change_the_postgresql_schema()
    {
        DbContextOptions<ProductDbContext> options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseNpgsql("Host=unused;Database=unused;Username=unused;Password=unused")
            .Options;
        using ProductDbContext db = new ProductDbContext(options);

        Assert.False(db.Database.HasPendingModelChanges());
    }

    private static async Task<Dictionary<string, object?>> CreateRequestAsync(ProductTestApp app)
    {
        (Guid brandId, Guid categoryId) = await app.AddReferencesAsync("Apple", "Laptop");

        return new Dictionary<string, object?>
        {
            ["name"] = "  MacBook Air  ",
            ["description"] = "Mô tả sản phẩm",
            ["price"] = 25_000_000L,
            ["brandId"] = brandId,
            ["categoryId"] = categoryId
        };
    }

    [Theory]
    [InlineData("Business Admin")]
    [InlineData("Technical Admin")]
    public async Task Administrators_can_create_and_persist_a_product(string role)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        Dictionary<string, object?> request = await CreateRequestAsync(app);
        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, Endpoint,
            Guid.NewGuid(), request, role);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("Thêm sản phẩm thành công.", json.RootElement.GetProperty("message").GetString());
        JsonElement data = json.RootElement.GetProperty("data");
        Guid id = data.GetProperty("id").GetGuid();
        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal("MacBook Air", data.GetProperty("name").GetString());
        Assert.Equal("ACTIVE", data.GetProperty("status").GetString());
        Assert.Equal(25_000_000L, data.GetProperty("price").GetInt64());
        Assert.Equal((Guid)request["brandId"]!, data.GetProperty("brandId").GetGuid());
        Assert.Equal((Guid)request["categoryId"]!, data.GetProperty("categoryId").GetGuid());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("updatedAt").ValueKind);

        await app.WithDatabaseAsync(async db =>
        {
            ProductDbModel product = await db.Products.SingleAsync();
            Assert.Equal(id, product.Id);
            Assert.Equal("MacBook Air", product.Name);
            Assert.Equal("Mô tả sản phẩm", product.Description);
            Assert.Equal(25_000_000L, product.Price);
            Assert.Equal(ProductStatus.ACTIVE, product.Status);
            Assert.Equal((Guid)request["brandId"]!, product.BrandId);
            Assert.Equal((Guid)request["categoryId"]!, product.CategoryId);
            Assert.Equal(TimeSpan.Zero, product.CreatedAt.Offset);
            Assert.Null(product.UpdatedAt);
            Assert.Equal(1, await db.Brands.CountAsync());
            Assert.Equal(1, await db.Categorys.CountAsync());
            Assert.Empty(await db.ProductImages.ToListAsync());
            return true;
        });
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("Customer", HttpStatusCode.Forbidden)]
    [InlineData("Staff", HttpStatusCode.Forbidden)]
    public async Task Unauthorized_roles_cannot_insert_products(string? role, HttpStatusCode expected)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        Dictionary<string, object?> request = await CreateRequestAsync(app);
        Guid? actor = null;
        if (role != null)
        {
            actor = Guid.NewGuid();
        }

        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, Endpoint, actor, request, role);
        Assert.Equal(expected, response.StatusCode);
        Assert.Equal(0, await app.WithDatabaseAsync(db => db.Products.CountAsync()));
    }

    [Theory]
    [InlineData("name", null)]
    [InlineData("name", "")]
    [InlineData("name", "   ")]
    [InlineData("description", null)]
    [InlineData("price", -1L)]
    [InlineData("brandId", "00000000-0000-0000-0000-000000000000")]
    [InlineData("categoryId", "00000000-0000-0000-0000-000000000000")]
    public async Task Validation_rejects_invalid_fields_before_inserting(string field, object? value)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        Dictionary<string, object?> request = await CreateRequestAsync(app);
        request[field] = value;
        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, Endpoint,
            Guid.NewGuid(), request, "Business Admin");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("BAD_REQUEST", json.RootElement.GetProperty("code").GetString());
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(0, await app.WithDatabaseAsync(db => db.Products.CountAsync()));
    }

    [Theory]
    [InlineData(255, HttpStatusCode.Created)]
    [InlineData(256, HttpStatusCode.BadRequest)]
    public async Task Name_length_boundary_is_enforced(int length, HttpStatusCode expected)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        Dictionary<string, object?> request = await CreateRequestAsync(app);
        request["name"] = new string('a', length);
        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, Endpoint,
            Guid.NewGuid(), request, "Business Admin");
        Assert.Equal(expected, response.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0L)]
    [InlineData(long.MaxValue)]
    public async Task Price_accepts_null_zero_and_bigint_maximum(long? price)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        Dictionary<string, object?> request = await CreateRequestAsync(app);
        request["price"] = price;
        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, Endpoint,
            Guid.NewGuid(), request, "Business Admin");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(price, await app.WithDatabaseAsync(db => db.Products.Select(row => row.Price).SingleAsync()));
    }

    [Theory]
    [InlineData("brandId")]
    [InlineData("categoryId")]
    public async Task Missing_references_return_not_found_without_inserting(string field)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        Dictionary<string, object?> request = await CreateRequestAsync(app);
        request[field] = Guid.NewGuid();
        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, Endpoint,
            Guid.NewGuid(), request, "Business Admin");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("NOT_FOUND", json.RootElement.GetProperty("code").GetString());
        Assert.Equal(0, await app.WithDatabaseAsync(db => db.Products.CountAsync()));
    }

    [Theory]
    [InlineData("price", "not-a-price")]
    [InlineData("price", 1.5)]
    [InlineData("brandId", "not-a-guid")]
    [InlineData("categoryId", "not-a-guid")]
    public async Task Invalid_json_field_types_are_rejected_without_inserting(string field, object value)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        Dictionary<string, object?> request = await CreateRequestAsync(app);
        request[field] = value;
        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, Endpoint,
            Guid.NewGuid(), request, "Business Admin");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await app.WithDatabaseAsync(db => db.Products.CountAsync()));
    }

    [Fact]
    public async Task Client_cannot_set_identity_status_or_audit_timestamps()
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        Dictionary<string, object?> request = await CreateRequestAsync(app);
        Guid clientId = Guid.NewGuid();
        request["id"] = clientId;
        request["status"] = "DISCONTINUED";
        request["createdAt"] = "2000-01-01T00:00:00Z";
        request["updatedAt"] = "2000-01-01T00:00:00Z";
        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, Endpoint,
            Guid.NewGuid(), request, "Business Admin");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        ProductDbModel stored = await app.WithDatabaseAsync(db => db.Products.AsNoTracking().SingleAsync());
        Assert.NotEqual(clientId, stored.Id);
        Assert.Equal(ProductStatus.ACTIVE, stored.Status);
        Assert.True(stored.CreatedAt > DateTimeOffset.Parse("2000-01-01T00:00:00Z"));
        Assert.Null(stored.UpdatedAt);
    }

    [Fact]
    public async Task Repeated_creation_inserts_new_products_without_overwriting_existing_rows()
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        Dictionary<string, object?> request = await CreateRequestAsync(app);
        using HttpResponseMessage first = await app.SendAsync(HttpMethod.Post, Endpoint,
            Guid.NewGuid(), request, "Business Admin");
        using HttpResponseMessage second = await app.SendAsync(HttpMethod.Post, Endpoint,
            Guid.NewGuid(), request, "Business Admin");
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        Assert.Equal(2, await app.WithDatabaseAsync(db => db.Products.CountAsync()));
    }

    [Theory]
    [InlineData(false, HttpStatusCode.InternalServerError)]
    [InlineData(true, HttpStatusCode.Conflict)]
    public async Task Storage_failure_does_not_persist_or_expose_technical_details(bool foreignKeyFailure,
        HttpStatusCode expected)
    {
        await using ProductTestApp app = await ProductTestApp.CreateAsync();
        Dictionary<string, object?> request = await CreateRequestAsync(app);
        if (foreignKeyFailure)
        {
            app.SaveFailure.Failure = new DbUpdateException("secret SQL detail",
                new PostgresException("secret constraint detail", "ERROR", "ERROR", PostgresErrorCodes.ForeignKeyViolation));
        }
        else
        {
            app.SaveFailure.FailWrites = true;
        }

        using HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, Endpoint,
            Guid.NewGuid(), request, "Business Admin");
        Assert.Equal(expected, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("secret", body);
        Assert.DoesNotContain("Simulated storage failure", body);
        Assert.Equal(0, await app.WithDatabaseAsync(db => db.Products.CountAsync()));
    }
}
