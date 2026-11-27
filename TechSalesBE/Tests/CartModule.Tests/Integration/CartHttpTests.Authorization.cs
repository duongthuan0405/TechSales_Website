using System.Text.Json;
using Xunit;

namespace CartModule.Tests.Integration;

public sealed partial class CartHttpTests
{
    [CartPostgresFact(DisplayName = "CART-AUTH-01 All Cart endpoints reject missing, expired or invalid credentials")]
    public async Task RejectsUnauthenticatedCartRequests()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 4);
        string token = actor.Token;
        int signatureStart = token.LastIndexOf('.') + 1;
        char replacement = token[signatureStart] == 'A' ? 'B' : 'A';
        string invalidSignature = token[..signatureStart] + replacement + token[(signatureStart + 1)..];
        Guid unknownUser = Guid.NewGuid();
        foreach (string? rejected in new[]
        {
            null,
            _app.CreateToken(actor.Id.ToString(), expired: true),
            invalidSignature,
            _app.CreateToken(subject: null),
            _app.CreateToken("not-a-guid"),
            _app.CreateToken(Guid.Empty.ToString()),
            _app.CreateToken(unknownUser.ToString())
        })
        {
            await AssertAllUnauthorized(rejected);
        }

        Assert.Equal(cart, await ReadCart(actor.Id));
        Assert.Equal(new StoredItem(Product, 4, Created, Updated), Assert.Single(await ReadItems(cart)));
        Assert.Null(await ReadCart(Guid.Empty));
        Assert.Null(await ReadCart(unknownUser));
    }

    [CartPostgresFact(DisplayName = "CART-AUTH-02 Inactivating an account rejects its previously valid JWT")]
    public async Task RejectsAValidTokenAfterAccountInactivation()
    {
        CartActor actor = await _app.CreateActorAsync();
        Guid cart = await SeedCart(actor.Id);
        await SeedItem(cart, Product, 4);
        Ok(await Get(actor.Token), CartRetrieved);
        Assert.Equal(1, await _app.ExecuteAsync("""
            UPDATE identify_access."User" SET "Status" = 'INACTIVE' WHERE "Id" = $1
            """, actor.Id));
        await AssertAllUnauthorized(actor.Token);
        Assert.Equal(cart, await ReadCart(actor.Id));
        Assert.Equal(new StoredItem(Product, 4, Created, Updated), Assert.Single(await ReadItems(cart)));
    }

    [CartPostgresFact(DisplayName = "CART-FLOW-01 Each authenticated role can complete the cart item lifecycle")]
    public async Task CompletesTheCartLifecycleForEachAuthenticatedRole()
    {
        foreach (string role in new[] { "Customer", "Staff", "Business Admin", "Technical Admin" })
        {
            CartActor actor = await _app.CreateActorAsync(role);
            JsonElement empty = Ok(await Get(actor.Token), CartRetrieved);
            Guid cart = empty.GetProperty("cartId").GetGuid();
            Assert.Empty(empty.GetProperty("items").EnumerateArray());

            ItemResult(await Add(actor.Token, Product, 2), ItemAdded, cart, Product, 2);
            ItemResult(await Put(actor.Token, Product, 1), ItemUpdated, cart, Product, 1);
            Assert.Equal(1, ReadResponseItem(Assert.Single(
                Ok(await Get(actor.Token), CartRetrieved).GetProperty("items").EnumerateArray())).Quantity);
            RemovedResult(await Delete(actor.Token, Product), Product);
            Assert.Empty(await ReadItems(cart));
            ItemResult(await Add(actor.Token, Product, 3), ItemAdded, cart, Product, 3);
            Assert.Equal(cart, await ReadCart(actor.Id));
            StoredItem final = Assert.Single(await ReadItems(cart));
            Assert.Equal(3, final.Quantity);
            Assert.Equal(final.CreatedAt, final.UpdatedAt);
        }
    }

    private async Task AssertAllUnauthorized(string? token)
    {
        Error(await Get(token), 401, "UNAUTHORIZED");
        Error(await Add(token, Product, 2), 401, "UNAUTHORIZED");
        Error(await Put(token, Product, 2), 401, "UNAUTHORIZED");
        Error(await Delete(token, Product), 401, "UNAUTHORIZED");
    }
}
