using CartModule.Application.Repositories;
using CartModule.Application.Services;
using CartModule.Domain.Entities;
using MediatR;

namespace CartModule.Application.UseCases.Carts.RemoveCartItem;

internal sealed class RemoveCartItem_UCHandler : IRequestHandler<RemoveCartItem_UCInput, RemoveCartItem_UCOutput>
{
    private readonly ICartRepository _carts;
    private readonly IExecuteAtomically _transaction;

    public RemoveCartItem_UCHandler(ICartRepository carts, IExecuteAtomically transaction)
    {
        _carts = carts;
        _transaction = transaction;
    }

    public Task<RemoveCartItem_UCOutput> Handle(RemoveCartItem_UCInput input, CancellationToken cancellationToken)
    {
        return _transaction.ExecuteAtomicallyAsync(async () =>
        {
            Cart? cart = await _carts.GetByUserIdForUpdateAsync(input.UserId, cancellationToken);
            if (cart != null && cart.RemoveItem(input.ProductId))
            {
                await _carts.RemoveItemAsync(cart.Id, input.ProductId, cancellationToken);
            }

            return new RemoveCartItem_UCOutput(input.ProductId);
        }, cancellationToken);
    }
}
