using CartModule.Application.Repositories;
using CartModule.Application.Services;
using CartModule.Application.UseCases.Carts.AddCartItem;
using CartModule.Application.UseCases.Carts.GetCart;
using CartModule.Domain.Entities;
using Xunit;

namespace CartModule.Tests;

public sealed class CartCreateConflictTests
{
    [Theory(DisplayName = "CART-UNIT-03 Stops when the winner of a concurrent insert cannot be reloaded")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DoesNotContinueWhenTheWinningCartDisappears(bool add)
    {
        var store = new MissingCartAfterConflict();
        Guid user = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");
        Guid product = Guid.Parse("11111111-1111-4111-8111-111111111111");
        Task request;
        if (add)
        {
            request = new AddCartItem_UCHandler(store, new ImmediateTransaction(), new CartProductsFake())
                .Handle(new AddCartItem_UCInput(user, product, 2), CancellationToken.None);
        }
        else
        {
            request = new GetCart_UCHandler(store, new CartProductsFake()).Handle(new GetCart_UCInput(user), CancellationToken.None);
        }

        InvalidOperationException failure = await Assert.ThrowsAsync<InvalidOperationException>(() => request);
        Assert.Equal("The existing cart could not be loaded after a concurrent insert.", failure.Message);
        string read = add ? "read with lock" : "read";
        Assert.Equal(new[] { (read, user), ("create", user), (read, user) }, store.Calls);
    }

    private sealed class MissingCartAfterConflict : ICartRepository
    {
        internal List<(string Operation, Guid User)> Calls { get; } = [];

        public Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            Calls.Add(("read", userId));
            return Task.FromResult<Cart?>(null);
        }

        public Task<Cart?> GetByUserIdForUpdateAsync(Guid userId, CancellationToken cancellationToken)
        {
            Calls.Add(("read with lock", userId));
            return Task.FromResult<Cart?>(null);
        }

        public Task<bool> TryCreateAsync(Cart cart, CancellationToken cancellationToken)
        {
            Calls.Add(("create", cart.UserId));
            return Task.FromResult(false);
        }

        public Task SaveItemAsync(CartItem item, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<bool> UpdateItemAsync(CartItem item, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task RemoveItemAsync(Guid cartId, Guid productId, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class ImmediateTransaction : IExecuteAtomically
    {
        public Task<T> ExecuteAtomicallyAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken) => action();
    }
}
