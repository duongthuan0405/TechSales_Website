using CartModule.Application.Repositories;
using CartModule.Application.Services;
using CartModule.Application.UseCases.Carts.AddCartItem;
using CartModule.Application.UseCases.Carts.GetCart;
using CartModule.Application.UseCases.Carts.UpdateCartItem;
using CartModule.Domain.Entities;
using CommonModule.BusinessExceptions;
using CommonModule.Domain.Exceptions;
using Xunit;

namespace CartModule.Tests;

public sealed class CartProductContractTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Missing_product_cannot_be_added_or_updated(bool update)
    {
        Store store = new Store();
        Guid id = Guid.NewGuid();
        CartProductsFake products = new CartProductsFake();
        products.MissingIds.Add(id);
        store.Cart.AddItem(id, DateTimeOffset.UtcNow);
        Task request = update
            ? new UpdateCartItem_UCHandler(store, store, products).Handle(new UpdateCartItem_UCInput(store.Cart.UserId, id, 2), default)
            : new AddCartItem_UCHandler(store, store, products).Handle(new AddCartItem_UCInput(store.Cart.UserId, id, 2), default);
        BusinessException error = await Assert.ThrowsAsync<BusinessException>(() => request);
        Assert.Equal(BusinessExceptionCode.NOT_FOUND, error.Code);
        Assert.Equal(0, store.Writes);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Discontinued_product_cannot_be_added_or_updated(bool update)
    {
        Store store = new Store();
        Guid id = Guid.NewGuid();
        store.Cart.AddItem(id, DateTimeOffset.UtcNow);
        CartProductsFake products = new CartProductsFake { Status = "DISCONTINUED" };
        Task request = update
            ? new UpdateCartItem_UCHandler(store, store, products).Handle(new UpdateCartItem_UCInput(store.Cart.UserId, id, 2), default)
            : new AddCartItem_UCHandler(store, store, products).Handle(new AddCartItem_UCInput(store.Cart.UserId, id, 2), default);
        await Assert.ThrowsAsync<DomainException>(() => request);
        Assert.Equal(0, store.Writes);
    }

    [Fact]
    public async Task Cart_get_batches_product_details_and_preserves_missing_items()
    {
        Store store = new Store();
        Guid available = Guid.NewGuid();
        Guid missing = Guid.NewGuid();
        store.Cart.AddItem(available, DateTimeOffset.UtcNow, 2);
        store.Cart.AddItem(missing, DateTimeOffset.UtcNow);
        CartProductsFake products = new CartProductsFake { Status = "DISCONTINUED" };
        products.MissingIds.Add(missing);
        GetCart_UCOutput result = await new GetCart_UCHandler(store, products).Handle(new GetCart_UCInput(store.Cart.UserId), default);
        Assert.Equal(2, result.Items.Count);
        Assert.Null(result.Items.Single(item => item.ProductId == missing).Product);
        var item = result.Items.Single(item => item.ProductId == available);
        Assert.Equal("Laptop", item.Product!.Name);
        Assert.Equal(15000000, item.Product.Price);
        Assert.Equal("DISCONTINUED", item.Product.Status);
        Assert.Single(item.Product.Images);
        Assert.Single(products.Calls);
        Assert.Equal(2, products.Calls[0].Count);
    }

    private sealed class Store : ICartRepository, IExecuteAtomically
    {
        public Cart Cart { get; } = new Cart(Guid.NewGuid(), Guid.NewGuid());
        public int Writes { get; private set; }
        public Task<Cart?> GetByUserIdAsync(Guid id, CancellationToken ct) => Task.FromResult<Cart?>(Cart);
        public Task<Cart?> GetByUserIdForUpdateAsync(Guid id, CancellationToken ct) => Task.FromResult<Cart?>(Cart);
        public Task<bool> TryCreateAsync(Cart cart, CancellationToken ct) => throw new NotSupportedException();
        public Task SaveItemAsync(CartItem item, CancellationToken ct) { Writes++; return Task.CompletedTask; }
        public Task<bool> UpdateItemAsync(CartItem item, CancellationToken ct) { Writes++; return Task.FromResult(true); }
        public Task RemoveItemAsync(Guid cartId, Guid productId, CancellationToken ct) => throw new NotSupportedException();
        public Task<T> ExecuteAtomicallyAsync<T>(Func<Task<T>> action, CancellationToken ct) => action();
    }
}
