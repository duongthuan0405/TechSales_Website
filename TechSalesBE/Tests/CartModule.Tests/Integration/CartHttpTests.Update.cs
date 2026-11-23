using Xunit;

namespace CartModule.Tests.Integration;

public sealed partial class CartHttpTests
{
    private const string ItemNotFound = "The product was not found in the cart.";

    [CartPostgresFact(DisplayName = "CART-UPDATE-01 Replaces quantity and updates only the target item")]
    public async Task UpdatesQuantityAndTimestamp()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 5);
        await SeedItem(cart, OtherProduct, 8);
        ItemResult(await Put(actor.Token, Product, 2), ItemUpdated, cart, Product, 2);
        Assert.Collection(await ReadItems(cart),
            item =>
            {
                Assert.Equal(Product, item.ProductId);
                Assert.Equal(2, item.Quantity);
                Assert.Equal(Created, item.CreatedAt);
                Assert.True(item.UpdatedAt > Updated);
            },
            item => Assert.Equal(new StoredItem(OtherProduct, 8, Created, Updated), item));
    }

    [CartPostgresFact(DisplayName = "CART-UPDATE-02 Repeating the current quantity does not write the row")]
    public async Task SameQuantityDoesNotTouchTimestamps()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 2);
        // A trigger that rejects every UPDATE distinguishes a true no-op from a redundant write.
        await _app.ExecuteAsync("""
            CREATE FUNCTION cart.reject_unchanged_fixture() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN RAISE EXCEPTION 'unexpected write'; END
            $$;
            CREATE TRIGGER reject_unchanged_fixture BEFORE UPDATE ON cart."CartItem"
            FOR EACH ROW EXECUTE FUNCTION cart.reject_unchanged_fixture();
            """);
        try
        {
            ItemResult(await Put(actor.Token, Product, 2), ItemUpdated, cart, Product, 2);
            ItemResult(await Put(actor.Token, Product, 2), ItemUpdated, cart, Product, 2);
            Assert.Equal(new StoredItem(Product, 2, Created, Updated), Assert.Single(await ReadItems(cart)));
        }
        finally
        {
            await _app.ExecuteAsync("""DROP TRIGGER reject_unchanged_fixture ON cart."CartItem"; DROP FUNCTION cart.reject_unchanged_fixture()""");
        }
    }

    [CartPostgresFact(DisplayName = "CART-UPDATE-03 Does not create a missing cart")]
    public async Task ReturnsNotFoundForAMissingCart()
    {
        CartActor actor = await _app.CreateActorAsync();
        Error(await Put(actor.Token, Product, 2), 404, "NOT_FOUND", "The cart was not found for the current user.");
        Assert.Null(await ReadCart(actor.Id));
    }

    [CartPostgresFact(DisplayName = "CART-UPDATE-04 Cannot update an item in another user's cart")]
    public async Task DoesNotUpdateAnotherUsersItem()
    {
        CartActor a = await _app.CreateActorAsync();
        CartActor b = await _app.CreateActorAsync();
        Guid cartA = await SeedCart(a.Id);
        Guid cartB = await SeedCart(b.Id);
        await SeedItem(cartA, Product, 5);
        await SeedItem(cartB, OtherProduct, 7);
        CartHttpResult response = await _app.SendAsync(HttpMethod.Put, $"{ItemsPath}/{Product}", b.Token,
            new { quantity = 2, userId = a.Id });
        Error(response, 404, "NOT_FOUND", ItemNotFound);
        Assert.Equal(new StoredItem(Product, 5, Created, Updated), Assert.Single(await ReadItems(cartA)));
        Assert.Equal(new StoredItem(OtherProduct, 7, Created, Updated), Assert.Single(await ReadItems(cartB)));
    }

    [CartPostgresFact(DisplayName = "CART-UPDATE-06 Rejects a missing or nonpositive quantity and an empty product ID")]
    public async Task RejectsInvalidUpdateFields()
    {
        const string invalidQuantity = "The new quantity must be a positive integer.";
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 5);
        ValidationError(await _app.SendAsync(HttpMethod.Put, $"{ItemsPath}/{Product}", actor.Token, new { }),
            "Quantity", invalidQuantity);
        foreach (int quantity in new[] { -1, 0 })
        {
            ValidationError(await Put(actor.Token, Product, quantity), "Quantity", invalidQuantity);
        }
        ValidationError(await Put(actor.Token, Guid.Empty, 2), "ProductId", InvalidProductId);
        Assert.Equal(new StoredItem(Product, 5, Created, Updated), Assert.Single(await ReadItems(cart)));
    }

    [CartPostgresFact(DisplayName = "CART-UPDATE-07 Rejects bad route IDs, malformed payloads and unsupported media")]
    public async Task RejectsInvalidPutRequests()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 5);
        Error(await _app.SendAsync(HttpMethod.Put, $"{ItemsPath}/not-a-guid", actor.Token, new { quantity = 2 }),
            400, "BAD_REQUEST");
        foreach (string json in new[] { "", "{", "null", """{"quantity":null}""", """{"quantity":1.5}""", """{"quantity":2147483648}""" })
        {
            Error(await _app.SendAsync(HttpMethod.Put, $"{ItemsPath}/{Product}", actor.Token, Text(json)), 400, "BAD_REQUEST");
        }
        Assert.Equal(415, (await _app.SendAsync(HttpMethod.Put, $"{ItemsPath}/{Product}", actor.Token, Text("plain", "text/plain"))).Code);
        Assert.Equal(new StoredItem(Product, 5, Created, Updated), Assert.Single(await ReadItems(cart)));
    }

    [CartPostgresFact(DisplayName = "CART-UPDATE-08 Concurrent PUT and POST produce a consistent serial order")]
    public async Task SerializesUpdateWithAdd()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 2);
        Task<CartHttpResult> update = Put(actor.Token, Product, 10);
        Task<CartHttpResult> add = Add(actor.Token, Product, 3);
        await Task.WhenAll(update, add);

        ItemResult(await update, ItemUpdated, cart, Product, 10);
        CartHttpResult addition = await add;
        int addedTotal = Ok(addition, ItemAdded).GetProperty("quantity").GetInt32();
        Assert.Contains(addedTotal, new[] { 5, 13 });
        ItemResult(addition, ItemAdded, cart, Product, addedTotal);
        StoredItem item = Assert.Single(await ReadItems(cart));
        Assert.Equal(addedTotal == 5 ? 10 : 13, item.Quantity);
        Assert.Equal(Created, item.CreatedAt);
    }

    [CartPostgresFact(DisplayName = "CART-UPDATE-09 Rolls back a failed quantity change")]
    public async Task FailedUpdatePreservesQuantityAndTimestamps()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 5);
        await _app.ExecuteAsync("""ALTER TABLE cart."CartItem" ADD CONSTRAINT reject_update_fixture CHECK ("Quantity" <> 99)""");
        try
        {
            Error(await Put(actor.Token, Product, 99), 500, "INTERNAL_SERVER_ERROR");
            Assert.Equal(new StoredItem(Product, 5, Created, Updated), Assert.Single(await ReadItems(cart)));
        }
        finally
        {
            await _app.ExecuteAsync("""ALTER TABLE cart."CartItem" DROP CONSTRAINT reject_update_fixture""");
        }
    }

    [CartPostgresFact(DisplayName = "CART-UPDATE-11 Accepts both quantity boundaries")]
    public async Task AcceptsMinimumAndMaximumQuantity()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 5);
        foreach (int quantity in new[] { 1, int.MaxValue })
        {
            ItemResult(await Put(actor.Token, Product, quantity), ItemUpdated, cart, Product, quantity);
            StoredItem item = Assert.Single(await ReadItems(cart));
            Assert.Equal(quantity, item.Quantity);
            Assert.Equal(Created, item.CreatedAt);
        }
    }

    [CartPostgresFact(DisplayName = "CART-UPDATE-12 Reports NOT_FOUND when the repository affects zero rows")]
    public async Task ReturnsNotFoundWhenNoRowWasUpdated()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 5);
        await _app.ExecuteAsync("""
            CREATE FUNCTION cart.skip_update_fixture() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN RETURN NULL; END
            $$;
            CREATE TRIGGER skip_update_fixture BEFORE UPDATE ON cart."CartItem"
            FOR EACH ROW EXECUTE FUNCTION cart.skip_update_fixture();
            """);
        try
        {
            Error(await Put(actor.Token, Product, 2), 404, "NOT_FOUND", ItemNotFound);
            Assert.Equal(new StoredItem(Product, 5, Created, Updated), Assert.Single(await ReadItems(cart)));
        }
        finally
        {
            await _app.ExecuteAsync("""DROP TRIGGER skip_update_fixture ON cart."CartItem"; DROP FUNCTION cart.skip_update_fixture()""");
        }
    }

    [CartPostgresFact(DisplayName = "CART-UPDATE-13 Rejects a change older than the previously stored timestamp")]
    public async Task RejectsTimestampRegression()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        DateTimeOffset future = new(2099, 1, 1, 8, 0, 0, TimeSpan.Zero);
        await SeedItem(cart, Product, 5, updated: future);
        Error(await Put(actor.Token, Product, 2), 422, "CART_INVALID_TIMESTAMP",
            "The cart item update time must be at or after the previous timestamp.");
        Assert.Equal(new StoredItem(Product, 5, Created, future), Assert.Single(await ReadItems(cart)));
    }
}
