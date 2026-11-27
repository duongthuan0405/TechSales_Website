using CartModule.Application.Repositories;
using CartModule.Application.Services;
using CartModule.Domain.Entities;
using MediatR;
using CartModule.Application.Ports;
using CartModule.Application.Exceptions;
using CartModule.Domain.Rules;
using CommonModule.BusinessExceptions;

namespace CartModule.Application.UseCases.Carts.AddCartItem;

internal sealed class AddCartItem_UCHandler : IRequestHandler<AddCartItem_UCInput, AddCartItem_UCOutput>
{
    private readonly ICartRepository _carts;
    private readonly IExecuteAtomically _transaction;
    private readonly ICartProducts _products;

    public AddCartItem_UCHandler(ICartRepository carts, IExecuteAtomically transaction, ICartProducts products)
    {
        _carts = carts;
        _transaction = transaction;
        _products = products;
    }

    public Task<AddCartItem_UCOutput> Handle(AddCartItem_UCInput input, CancellationToken cancellationToken)
    {
        return _transaction.ExecuteAtomicallyAsync(async () =>
        {
            CartProduct? product = (await _products.GetProductsAsync(new List<Guid> { input.ProductId }, cancellationToken))
                .SingleOrDefault();
            if (product == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.ProductNotFound);
            }
            CartRules.EnsureProductPurchasable(product.Status);
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
