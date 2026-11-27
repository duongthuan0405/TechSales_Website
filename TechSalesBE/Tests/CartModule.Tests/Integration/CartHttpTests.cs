using System.Text;
using System.Text.Json;
using Npgsql;
using Xunit;

namespace CartModule.Tests.Integration;

public sealed partial class CartHttpTests : IClassFixture<CartTestApp>
{
    private const string CartPath = "/api/cart";
    private const string ItemsPath = "/api/cart/items";
    private const string CartRetrieved = "Cart retrieved successfully.";
    private const string ItemAdded = "Item added to cart successfully.";
    private const string ItemUpdated = "Cart item quantity updated successfully.";
    private const string ItemRemoved = "Cart item removed successfully.";
    private const string InvalidProductId = "Product ID must be a non-empty UUID.";
    private static readonly Guid Product = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid OtherProduct = Guid.Parse("22222222-2222-4222-8222-222222222222");
    private static readonly Guid ThirdProduct = Guid.Parse("33333333-3333-4333-8333-333333333333");
    private static readonly DateTimeOffset Created = new(2025, 1, 1, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Updated = Created.AddMinutes(5);
    private readonly CartTestApp _app;

    public CartHttpTests(CartTestApp app)
    {
        _app = app;
    }

    private Task<CartHttpResult> Get(string? token, string query = "") =>
        _app.SendAsync(HttpMethod.Get, CartPath + query, token);

    private Task<CartHttpResult> Add(string? token, Guid product, int quantity) =>
        _app.SendAsync(HttpMethod.Post, ItemsPath, token, new { productId = product, quantity });

    private Task<CartHttpResult> Put(string? token, Guid product, int quantity) =>
        _app.SendAsync(HttpMethod.Put, $"{ItemsPath}/{product}", token, new { quantity });

    private Task<CartHttpResult> Delete(string? token, Guid product, string query = "") =>
        _app.SendAsync(HttpMethod.Delete, $"{ItemsPath}/{product}{query}", token);

    private static StringContent Text(string body, string contentType = "application/json") =>
        new(body, Encoding.UTF8, contentType);

    private static JsonElement Ok(CartHttpResult response, string message)
    {
        Assert.Equal(200, response.Code);
        Assert.True(response.Json.GetProperty("success").GetBoolean());
        Assert.Equal(message, response.Json.GetProperty("message").GetString());
        return response.Data;
    }

    private static void Error(CartHttpResult response, int status, string code, string? message = null)
    {
        Assert.Equal(status, response.Code);
        Assert.False(response.Json.GetProperty("success").GetBoolean());
        Assert.Equal(code, response.Json.GetProperty("code").GetString());
        if (message != null)
        {
            Assert.Equal(message, response.Json.GetProperty("message").GetString());
        }
    }

    private static void ValidationError(CartHttpResult response, string field, string message)
    {
        Error(response, 400, "BAD_REQUEST");
        JsonElement detail = Assert.Single(response.Data.GetProperty(field).EnumerateArray());
        Assert.Equal(message, detail.GetString());
    }

    private static void ItemResult(CartHttpResult response, string message, Guid cart, Guid product, int quantity)
    {
        JsonElement data = Ok(response, message);
        Assert.Equal(cart, data.GetProperty("cartId").GetGuid());
        Assert.Equal(product, data.GetProperty("productId").GetGuid());
        Assert.Equal(quantity, data.GetProperty("quantity").GetInt32());
    }

    private static void RemovedResult(CartHttpResult response, Guid product) =>
        Assert.Equal(product, Ok(response, ItemRemoved).GetProperty("productId").GetGuid());

    private async Task<Guid> SeedCart(Guid userId)
    {
        Guid cartId = Guid.NewGuid();
        await _app.ExecuteAsync("""INSERT INTO cart."Cart" ("Id", "UserId") VALUES ($1, $2)""", cartId, userId);
        return cartId;
    }

    private Task<int> SeedItem(Guid cart, Guid product, int quantity, DateTimeOffset? created = null, DateTimeOffset? updated = null) =>
        _app.ExecuteAsync("""
            INSERT INTO cart."CartItem" ("CartId", "ProductId", "Quantity", "CreatedAt", "UpdatedAt")
            VALUES ($1, $2, $3, $4, $5)
            """, cart, product, quantity, created ?? Created, updated ?? Updated);

    private async Task<Guid?> ReadCart(Guid userId)
    {
        await using NpgsqlConnection connection = await _app.OpenAsync();
        await using NpgsqlCommand command = CartTestApp.Command(connection,
            """SELECT "Id" FROM cart."Cart" WHERE "UserId" = $1""", [userId]);
        await using NpgsqlDataReader data = await command.ExecuteReaderAsync();
        if (!await data.ReadAsync())
        {
            return null;
        }

        Guid id = data.GetGuid(0);
        Assert.False(await data.ReadAsync(), "The actor can own at most one cart.");
        return id;
    }

    private async Task<List<StoredItem>> ReadItems(Guid cart)
    {
        await using NpgsqlConnection connection = await _app.OpenAsync();
        await using NpgsqlCommand command = CartTestApp.Command(connection, """
            SELECT "ProductId", "Quantity", "CreatedAt", "UpdatedAt" FROM cart."CartItem"
            WHERE "CartId" = $1 ORDER BY "ProductId"
            """, [cart]);
        await using NpgsqlDataReader data = await command.ExecuteReaderAsync();
        List<StoredItem> items = [];
        while (await data.ReadAsync())
        {
            items.Add(new StoredItem(data.GetGuid(0), data.GetInt32(1),
                data.GetFieldValue<DateTimeOffset>(2), data.GetFieldValue<DateTimeOffset>(3)));
        }
        return items;
    }

    private static StoredItem ReadResponseItem(JsonElement item) => new(
        item.GetProperty("productId").GetGuid(), item.GetProperty("quantity").GetInt32(),
        item.GetProperty("createdAt").GetDateTimeOffset(), item.GetProperty("updatedAt").GetDateTimeOffset());

    private sealed record StoredItem(Guid ProductId, int Quantity, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
}
