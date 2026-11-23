using System.Text.Json;
using Npgsql;
using Xunit;

namespace CartModule.Tests.Integration;

public sealed partial class CartHttpTests
{
    [CartPostgresFact(DisplayName = "CART-ADD-01 Creates a cart and defaults the first item to one")]
    public async Task AddsTheFirstItemWithTheDefaultQuantity()
    {
        CartActor actor = await _app.CreateActorAsync();
        CartHttpResult response = await _app.SendAsync(HttpMethod.Post, ItemsPath, actor.Token, new { productId = Product });
        Guid cart = Ok(response, ItemAdded).GetProperty("cartId").GetGuid();
        Assert.NotEqual(Guid.Empty, cart);
        ItemResult(response, ItemAdded, cart, Product, 1);
        Assert.Equal(cart, await ReadCart(actor.Id));
        StoredItem item = Assert.Single(await ReadItems(cart));
        Assert.Equal(Product, item.ProductId);
        Assert.Equal(1, item.Quantity);
        Assert.Equal(item.CreatedAt, item.UpdatedAt);
    }

    [CartPostgresFact(DisplayName = "CART-ADD-02 Increases an existing line without recreating it")]
    public async Task AddsToAnExistingItem()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 4);
        ItemResult(await Add(actor.Token, Product, 3), ItemAdded, cart, Product, 7);
        StoredItem item = Assert.Single(await ReadItems(cart));
        Assert.Equal(Product, item.ProductId);
        Assert.Equal(7, item.Quantity);
        Assert.Equal(Created, item.CreatedAt);
        Assert.True(item.UpdatedAt > Updated);
    }

    [CartPostgresFact(DisplayName = "CART-ADD-03 Adds a different product without changing existing items")]
    public async Task AddsASeparateItemToAnExistingCart()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 4);
        ItemResult(await Add(actor.Token, OtherProduct, 2), ItemAdded, cart, OtherProduct, 2);
        Assert.Collection(await ReadItems(cart),
            item => Assert.Equal(new StoredItem(Product, 4, Created, Updated), item),
            item =>
            {
                Assert.Equal(OtherProduct, item.ProductId);
                Assert.Equal(2, item.Quantity);
                Assert.Equal(item.CreatedAt, item.UpdatedAt);
            });
    }

    [CartPostgresFact(DisplayName = "CART-ADD-04 Ignores another user's ID in the request")]
    public async Task AddsOnlyToTheAuthenticatedCart()
    {
        CartActor a = await _app.CreateActorAsync();
        CartActor b = await _app.CreateActorAsync();
        Guid cartA = await SeedCart(a.Id);
        Guid cartB = await SeedCart(b.Id);
        await SeedItem(cartA, Product, 4);
        await SeedItem(cartB, Product, 8);
        var body = new { productId = Product, quantity = 2, userId = b.Id };
        ItemResult(await _app.SendAsync(HttpMethod.Post, ItemsPath, a.Token, body), ItemAdded, cartA, Product, 6);
        Assert.Equal(6, Assert.Single(await ReadItems(cartA)).Quantity);
        Assert.Equal(new StoredItem(Product, 8, Created, Updated), Assert.Single(await ReadItems(cartB)));
    }

    [CartPostgresFact(DisplayName = "CART-ADD-06 Rejects missing IDs and nonpositive quantities")]
    public async Task RejectsInvalidFieldsBeforeCreatingACart()
    {
        CartActor actor = await _app.CreateActorAsync();
        ValidationError(await _app.SendAsync(HttpMethod.Post, ItemsPath, actor.Token, new { quantity = 1 }),
            "ProductId", InvalidProductId);
        ValidationError(await Add(actor.Token, Guid.Empty, 1), "ProductId", InvalidProductId);
        foreach (int quantity in new[] { -1, 0 })
        {
            ValidationError(await Add(actor.Token, Product, quantity), "Quantity", "Quantity to add must be a positive integer.");
        }
        Assert.Null(await ReadCart(actor.Id));
    }

    [CartPostgresFact(DisplayName = "CART-ADD-07 Accepts the maximum total and rejects overflow without changes")]
    public async Task AcceptsTheMaximumTotalBeforeRejectingOverflow()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, int.MaxValue - 1);
        ItemResult(await Add(actor.Token, Product, 1), ItemAdded, cart, Product, int.MaxValue);
        StoredItem beforeOverflow = Assert.Single(await ReadItems(cart));
        Assert.Equal(int.MaxValue, beforeOverflow.Quantity);
        Assert.Equal(Created, beforeOverflow.CreatedAt);
        Assert.True(beforeOverflow.UpdatedAt > Updated);
        Error(await Add(actor.Token, Product, 1), 422, "CART_QUANTITY_OVERFLOW",
            "Cart item quantity exceeds the supported integer range.");
        Assert.Equal(beforeOverflow, Assert.Single(await ReadItems(cart)));
    }

    [CartPostgresFact(DisplayName = "CART-ADD-08 Concurrent first additions share one cart and preserve all increments")]
    public async Task ConcurrentAddsDoNotLoseIncrements()
    {
        CartActor actor = await _app.CreateActorAsync();
        CartHttpResult[] responses = await Task.WhenAll(Enumerable.Range(0, 12).Select(_ => Add(actor.Token, Product, 1)));
        Guid? cart = await ReadCart(actor.Id);
        Assert.NotNull(cart);
        List<int> totals = [];
        foreach (CartHttpResult response in responses)
        {
            JsonElement data = Ok(response, ItemAdded);
            Assert.Equal(cart, data.GetProperty("cartId").GetGuid());
            Assert.Equal(Product, data.GetProperty("productId").GetGuid());
            totals.Add(data.GetProperty("quantity").GetInt32());
        }
        Assert.Equal(Enumerable.Range(1, 12), totals.Order());
        StoredItem item = Assert.Single(await ReadItems(cart.Value));
        Assert.Equal(Product, item.ProductId);
        Assert.Equal(12, item.Quantity);
    }

    [CartPostgresFact(DisplayName = "CART-ADD-09 GET and POST racing on first use share one cart")]
    public async Task FirstGetAndAddShareACart()
    {
        CartActor actor = await _app.CreateActorAsync();
        Task<CartHttpResult> read = Get(actor.Token);
        Task<CartHttpResult> add = Add(actor.Token, Product, 2);
        await Task.WhenAll(read, add);

        JsonElement data = Ok(await read, CartRetrieved);
        Guid cart = data.GetProperty("cartId").GetGuid();
        ItemResult(await add, ItemAdded, cart, Product, 2);
        Assert.Equal(cart, await ReadCart(actor.Id));
        StoredItem persisted = Assert.Single(await ReadItems(cart));
        Assert.Equal(Product, persisted.ProductId);
        Assert.Equal(2, persisted.Quantity);
        JsonElement.ArrayEnumerator items = data.GetProperty("items").EnumerateArray();
        if (items.MoveNext())
        {
            Assert.Equal(persisted, ReadResponseItem(items.Current));
            Assert.False(items.MoveNext());
        }
    }

    [CartPostgresFact(DisplayName = "CART-ADD-10 Reads the latest quantity after waiting for the cart lock")]
    public async Task ReloadsItemsAfterTakingTheCartLock()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 4);
        await using NpgsqlConnection db = await _app.OpenAsync();
        await using NpgsqlTransaction blocker = await db.BeginTransactionAsync();
        await using var lockCart = CartTestApp.Command(db, """SELECT 1 FROM cart."Cart" WHERE "Id" = $1 FOR UPDATE""", [cart]);
        lockCart.Transaction = blocker;
        Assert.Equal(1, await lockCart.ExecuteScalarAsync());
        await using var setQuantity = CartTestApp.Command(db,
            """UPDATE cart."CartItem" SET "Quantity" = 9 WHERE "CartId" = $1 AND "ProductId" = $2""", [cart, Product]);
        setQuantity.Transaction = blocker;
        Assert.Equal(1, await setQuantity.ExecuteNonQueryAsync());
        Task<CartHttpResult> pending = Add(actor.Token, Product, 2);
        await _app.WaitForCartLockAsync();
        await blocker.CommitAsync();

        ItemResult(await pending, ItemAdded, cart, Product, 11);
        Assert.Equal(11, Assert.Single(await ReadItems(cart)).Quantity);
    }

    [CartPostgresFact(DisplayName = "CART-ADD-11 Rolls back a new cart when saving the first item fails")]
    public async Task FailedItemInsertRollsBackTheCart()
    {
        CartActor actor = await _app.CreateActorAsync();
        await _app.ExecuteAsync("""ALTER TABLE cart."CartItem" ADD CONSTRAINT reject_add_fixture CHECK ("Quantity" <> 99)""");
        try
        {
            Error(await Add(actor.Token, Product, 99), 500, "INTERNAL_SERVER_ERROR");
            Assert.Null(await ReadCart(actor.Id));
        }
        finally
        {
            await _app.ExecuteAsync("""ALTER TABLE cart."CartItem" DROP CONSTRAINT reject_add_fixture""");
        }
    }

    [CartPostgresFact(DisplayName = "CART-ADD-13 Rejects invalid JSON values and media types without writes")]
    public async Task RejectsInvalidPostBodies()
    {
        CartActor actor = await _app.CreateActorAsync();
        foreach (string json in new[]
        {
            "", "{", "null", """{"productId":"not-a-guid","quantity":1}""",
            $$"""{"productId":"{{Product}}","quantity":null}""",
            $$"""{"productId":"{{Product}}","quantity":1.5}""",
            $$"""{"productId":"{{Product}}","quantity":2147483648}"""
        })
        {
            Error(await _app.SendAsync(HttpMethod.Post, ItemsPath, actor.Token, Text(json)), 400, "BAD_REQUEST");
        }
        Assert.Equal(415, (await _app.SendAsync(HttpMethod.Post, ItemsPath, actor.Token, Text("plain", "text/plain"))).Code);
        Assert.Null(await ReadCart(actor.Id));
    }

    [CartPostgresFact(DisplayName = "CART-ADD-14 Accepts the largest positive quantity for a new line")]
    public async Task AcceptsMaximumQuantityOnInsert()
    {
        CartActor actor = await _app.CreateActorAsync();
        CartHttpResult response = await Add(actor.Token, Product, int.MaxValue);
        Guid cart = Ok(response, ItemAdded).GetProperty("cartId").GetGuid();
        ItemResult(response, ItemAdded, cart, Product, int.MaxValue);
        Assert.Equal(cart, await ReadCart(actor.Id));
        Assert.Equal(int.MaxValue, Assert.Single(await ReadItems(cart)).Quantity);
    }

    [CartPostgresFact(DisplayName = "CART-ADD-15 Adds to the item committed by a concurrent cart creator")]
    public async Task AddsToTheCartThatWonAConcurrentInsert()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = Guid.NewGuid();
        await using NpgsqlConnection db = await _app.OpenAsync();
        await using NpgsqlTransaction blocker = await db.BeginTransactionAsync();
        await using var insertCart = CartTestApp.Command(db, """INSERT INTO cart."Cart" ("Id", "UserId") VALUES ($1, $2)""", [cart, actor.Id]);
        insertCart.Transaction = blocker;
        await insertCart.ExecuteNonQueryAsync();
        await using var insertItem = CartTestApp.Command(db, """
            INSERT INTO cart."CartItem" ("CartId", "ProductId", "Quantity", "CreatedAt", "UpdatedAt")
            VALUES ($1, $2, 4, $3, $4)
            """, [cart, Product, Created, Updated]);
        insertItem.Transaction = blocker;
        await insertItem.ExecuteNonQueryAsync();

        Task<CartHttpResult> pending = Add(actor.Token, Product, 2);
        await _app.WaitForCartLockAsync();
        await blocker.CommitAsync();

        ItemResult(await pending, ItemAdded, cart, Product, 6);
        Assert.Equal(cart, await ReadCart(actor.Id));
        StoredItem item = Assert.Single(await ReadItems(cart));
        Assert.Equal(6, item.Quantity);
        Assert.Equal(Created, item.CreatedAt);
    }
}
