using CartModule.Application.Repositories;
using CartModule.Application.Services;
using CartModule.Domain.Entities;
using MediatR;

namespace CartModule.Application.UseCases.Carts.AddCartItem;

internal sealed class AddCartItem_UCHandler : IRequestHandler<AddCartItem_UCInput, AddCartItem_UCOutput>
{
    private readonly ICartRepository _carts;
    private readonly IExecuteAtomically _transaction;

    public AddCartItem_UCHandler(ICartRepository carts, IExecuteAtomically transaction)
    {
        _carts = carts;
        _transaction = transaction;
    }

    public Task<AddCartItem_UCOutput> Handle(AddCartItem_UCInput input, CancellationToken cancellationToken)
    {
        return _transaction.ExecuteAtomicallyAsync(async () =>
        {
            Cart? cart = await _carts.GetByUserIdForUpdateAsync(input.UserId, cancellationToken);
            if (cart == null)
            {
                Cart newCart = new Cart(Guid.NewGuid(), input.UserId);
                bool created = await _carts.TryCreateAsync(newCart, cancellationToken);
                if (created)
                {
                    cart = newCart;
                }
                else
                {
                    cart = await _carts.GetByUserIdForUpdateAsync(input.UserId, cancellationToken)
                        ?? throw new InvalidOperationException("The existing cart could not be loaded after a concurrent insert.");
                }
            }

            CartItem item = cart.AddItem(input.ProductId, DateTimeOffset.UtcNow, input.Quantity);
            await _carts.SaveItemAsync(item, cancellationToken);

            return new AddCartItem_UCOutput(item.CartId, item.ProductId, item.Quantity);
        }, cancellationToken);
    }
}
