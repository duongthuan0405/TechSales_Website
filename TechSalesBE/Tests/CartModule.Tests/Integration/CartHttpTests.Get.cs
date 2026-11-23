using System.Text.Json;
using Npgsql;
using Xunit;

namespace CartModule.Tests.Integration;

public sealed partial class CartHttpTests
{
    [CartPostgresFact(DisplayName = "CART-GET-01 Creates an empty cart on the first GET")]
    public async Task CreatesAnEmptyCart()
    {
        CartActor actor = await _app.CreateActorAsync();
        CartHttpResult response = await Get(actor.Token);
        JsonElement data = Ok(response, CartRetrieved);
        Guid id = data.GetProperty("cartId").GetGuid();
        Assert.NotEqual(Guid.Empty, id);
        Assert.Empty(data.GetProperty("items").EnumerateArray());
        Assert.True(response.NoStore);
        Assert.Equal(id, await ReadCart(actor.Id));
        Assert.Empty(await ReadItems(id));
    }

    [CartPostgresFact(DisplayName = "CART-GET-02 Reuses the empty cart on repeated GET")]
    public async Task ReusesAnExistingEmptyCart()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        foreach (CartHttpResult response in new[] { await Get(actor.Token), await Get(actor.Token) })
        {
            JsonElement data = Ok(response, CartRetrieved);
            Assert.Equal(cart, data.GetProperty("cartId").GetGuid());
            Assert.Empty(data.GetProperty("items").EnumerateArray());
        }
        Assert.Equal(cart, await ReadCart(actor.Id));
        Assert.Empty(await ReadItems(cart));
    }

    [CartPostgresFact(DisplayName = "CART-GET-03 Orders items by creation time then product and disables caching")]
    public async Task OrdersAndMapsItems()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, ThirdProduct, 4);
        await SeedItem(cart, Product, 8, Created.AddMinutes(1));
        await SeedItem(cart, OtherProduct, 2);
        List<StoredItem> before = await ReadItems(cart);
        CartHttpResult response = await Get(actor.Token);
        JsonElement data = Ok(response, CartRetrieved);
        Assert.Equal(cart, data.GetProperty("cartId").GetGuid());
        Assert.Equal(new[]
        {
            new StoredItem(OtherProduct, 2, Created, Updated),
            new StoredItem(ThirdProduct, 4, Created, Updated),
            new StoredItem(Product, 8, Created.AddMinutes(1), Updated)
        }, data.GetProperty("items").EnumerateArray().Select(ReadResponseItem));
        Assert.True(response.NoStore);
        Assert.Equal(before, await ReadItems(cart));
    }

    [CartPostgresFact(DisplayName = "CART-GET-04 Ignores a different userId in the query")]
    public async Task ReadsOnlyTheAuthenticatedCart()
    {
        CartActor a = await _app.CreateActorAsync();
        CartActor b = await _app.CreateActorAsync();
        Guid cartA = await SeedCart(a.Id);
        Guid cartB = await SeedCart(b.Id);
        await SeedItem(cartA, Product, 2);
        await SeedItem(cartB, OtherProduct, 7);

        JsonElement data = Ok(await Get(a.Token, $"?userId={b.Id}"), CartRetrieved);
        Assert.Equal(cartA, data.GetProperty("cartId").GetGuid());
        Assert.Equal(new StoredItem(Product, 2, Created, Updated),
            ReadResponseItem(Assert.Single(data.GetProperty("items").EnumerateArray())));
        Assert.Equal(new StoredItem(Product, 2, Created, Updated), Assert.Single(await ReadItems(cartA)));
        Assert.Equal(new StoredItem(OtherProduct, 7, Created, Updated), Assert.Single(await ReadItems(cartB)));
    }

    [CartPostgresFact(DisplayName = "CART-GET-07 Concurrent first GETs produce one cart")]
    public async Task ConcurrentFirstReadsShareACart()
    {
        CartActor actor = await _app.CreateActorAsync();
        CartHttpResult[] responses = await Task.WhenAll(Enumerable.Range(0, 12).Select(_ => Get(actor.Token)));
        Guid? cart = await ReadCart(actor.Id);
        Assert.NotNull(cart);
        foreach (CartHttpResult response in responses)
        {
            JsonElement data = Ok(response, CartRetrieved);
            Assert.Equal(cart, data.GetProperty("cartId").GetGuid());
            Assert.Empty(data.GetProperty("items").EnumerateArray());
        }
        Assert.Empty(await ReadItems(cart.Value));
    }

    [CartPostgresFact(DisplayName = "CART-GET-08 Loads the cart that won a concurrent insert")]
    public async Task ReloadsTheCartAfterAnInsertConflict()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = Guid.NewGuid();
        await using NpgsqlConnection db = await _app.OpenAsync();
        await using NpgsqlTransaction blocker = await db.BeginTransactionAsync();
        await using NpgsqlCommand insert = CartTestApp.Command(db,
            """INSERT INTO cart."Cart" ("Id", "UserId") VALUES ($1, $2)""", [cart, actor.Id]);
        insert.Transaction = blocker;
        await insert.ExecuteNonQueryAsync();
        Task<CartHttpResult> pending = Get(actor.Token);
        await _app.WaitForCartLockAsync();
        await blocker.CommitAsync();

        JsonElement data = Ok(await pending, CartRetrieved);
        Assert.Equal(cart, data.GetProperty("cartId").GetGuid());
        Assert.Empty(data.GetProperty("items").EnumerateArray());
        Assert.Equal(cart, await ReadCart(actor.Id));
    }

    [CartPostgresFact(DisplayName = "CART-GET-09 Failed cart insertion returns a generic 500")]
    public async Task FailedGetDoesNotCreateACart()
    {
        CartActor actor = await _app.CreateActorAsync();
        await _app.ExecuteAsync($"""ALTER TABLE cart."Cart" ADD CONSTRAINT reject_get_fixture CHECK ("UserId" <> '{actor.Id}'::uuid)""");
        try
        {
            CartHttpResult response = await Get(actor.Token);
            Error(response, 500, "INTERNAL_SERVER_ERROR");
            Assert.DoesNotContain("reject_get_fixture", response.Json.GetRawText());
            Assert.Null(await ReadCart(actor.Id));
        }
        finally
        {
            await _app.ExecuteAsync("""ALTER TABLE cart."Cart" DROP CONSTRAINT reject_get_fixture""");
        }
    }
}
