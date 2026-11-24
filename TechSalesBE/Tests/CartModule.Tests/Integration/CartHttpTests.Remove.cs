using System.Text.Json;
using Xunit;

namespace CartModule.Tests.Integration;

public sealed partial class CartHttpTests
{
    [CartPostgresFact(DisplayName = "CART-REMOVE-01 Deletes only the matching product line")]
    public async Task RemovesOnlyTheTargetItem()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 4);
        await SeedItem(cart, OtherProduct, 2);
        RemovedResult(await Delete(actor.Token, Product), Product);
        Assert.Equal(cart, await ReadCart(actor.Id));
        Assert.Equal(new StoredItem(OtherProduct, 2, Created, Updated), Assert.Single(await ReadItems(cart)));
    }

    [CartPostgresFact(DisplayName = "CART-REMOVE-02 Repeated DELETE succeeds and PUT on the removed item fails")]
    public async Task RepeatedDeleteIsIdempotent()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 4);
        await SeedItem(cart, OtherProduct, 2);
        RemovedResult(await Delete(actor.Token, Product), Product);
        RemovedResult(await Delete(actor.Token, Product), Product);
        Error(await Put(actor.Token, Product, 2), 404, "NOT_FOUND", ItemNotFound);
        Assert.Equal(new StoredItem(OtherProduct, 2, Created, Updated), Assert.Single(await ReadItems(cart)));
    }

    [CartPostgresFact(DisplayName = "CART-REMOVE-03 DELETE succeeds without creating a missing cart")]
    public async Task DeleteWithoutACartDoesNotCreateOne()
    {
        CartActor actor = await _app.CreateActorAsync();
        RemovedResult(await Delete(actor.Token, Product), Product);
        Assert.Null(await ReadCart(actor.Id));
    }

    [CartPostgresFact(DisplayName = "CART-REMOVE-04 Removes the item only for the authenticated user")]
    public async Task IgnoresAnotherUserIdWhenRemoving()
    {
        CartActor a = await _app.CreateActorAsync();
        CartActor b = await _app.CreateActorAsync();
        Guid cartA = await SeedCart(a.Id);
        Guid cartB = await SeedCart(b.Id);
        await SeedItem(cartA, Product, 4);
        await SeedItem(cartB, Product, 8);
        RemovedResult(await Delete(a.Token, Product, $"?userId={b.Id}"), Product);
        Assert.Equal(cartA, await ReadCart(a.Id));
        Assert.Empty(await ReadItems(cartA));
        Assert.Equal(new StoredItem(Product, 8, Created, Updated), Assert.Single(await ReadItems(cartB)));
    }

    [CartPostgresFact(DisplayName = "CART-REMOVE-05 Concurrent removals leave the same empty cart")]
    public async Task ConcurrentDeletesKeepTheEmptyCart()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, OtherProduct, 2);
        foreach (CartHttpResult response in await Task.WhenAll(Delete(actor.Token, OtherProduct), Delete(actor.Token, OtherProduct)))
        {
            RemovedResult(response, OtherProduct);
        }
        JsonElement data = Ok(await Get(actor.Token), CartRetrieved);
        Assert.Equal(cart, data.GetProperty("cartId").GetGuid());
        Assert.Empty(data.GetProperty("items").EnumerateArray());
        Assert.Equal(cart, await ReadCart(actor.Id));
        Assert.Empty(await ReadItems(cart));
    }

    [CartPostgresFact(DisplayName = "CART-REMOVE-07 Rejects malformed and empty product UUIDs")]
    public async Task RejectsInvalidDeleteRoutes()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 4);
        Error(await _app.SendAsync(HttpMethod.Delete, $"{ItemsPath}/not-a-guid", actor.Token), 400, "BAD_REQUEST");
        ValidationError(await Delete(actor.Token, Guid.Empty), "ProductId", InvalidProductId);
        Assert.Equal(new StoredItem(Product, 4, Created, Updated), Assert.Single(await ReadItems(cart)));
    }

    [CartPostgresFact(DisplayName = "CART-REMOVE-08 Concurrent DELETE and POST produce a consistent serial order")]
    public async Task SerializesDeleteWithAdd()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 4);
        Task<CartHttpResult> remove = Delete(actor.Token, Product);
        Task<CartHttpResult> add = Add(actor.Token, Product, 3);
        await Task.WhenAll(remove, add);

        RemovedResult(await remove, Product);
        CartHttpResult addition = await add;
        int addedTotal = Ok(addition, ItemAdded).GetProperty("quantity").GetInt32();
        Assert.Contains(addedTotal, new[] { 3, 7 });
        ItemResult(addition, ItemAdded, cart, Product, addedTotal);
        List<StoredItem> items = await ReadItems(cart);
        if (addedTotal == 3)
        {
            StoredItem recreated = Assert.Single(items);
            Assert.Equal(Product, recreated.ProductId);
            Assert.Equal(3, recreated.Quantity);
            Assert.True(recreated.CreatedAt > Updated);
            Assert.Equal(recreated.CreatedAt, recreated.UpdatedAt);
        }
        else
        {
            Assert.Empty(items);
        }
        Assert.Equal(cart, await ReadCart(actor.Id));
    }

    [CartPostgresFact(DisplayName = "CART-REMOVE-09 Rolls back deletion when another row references the item")]
    public async Task FailedDeleteLeavesTheOriginalItem()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 4);
        await _app.ExecuteAsync("""
            CREATE TABLE cart.delete_guard_fixture (
                cart_id uuid NOT NULL, product_id uuid NOT NULL,
                FOREIGN KEY (cart_id, product_id) REFERENCES cart."CartItem" ("CartId", "ProductId")
            )
            """);
        try
        {
            await _app.ExecuteAsync("INSERT INTO cart.delete_guard_fixture VALUES ($1, $2)", cart, Product);
            Error(await Delete(actor.Token, Product), 500, "INTERNAL_SERVER_ERROR");
            Assert.Equal(cart, await ReadCart(actor.Id));
            Assert.Equal(new StoredItem(Product, 4, Created, Updated), Assert.Single(await ReadItems(cart)));
        }
        finally
        {
            await _app.ExecuteAsync("DROP TABLE cart.delete_guard_fixture");
        }
    }
}
