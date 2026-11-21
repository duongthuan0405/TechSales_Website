using CartModule.Application.Exceptions;
using CartModule.Application.Repositories;
using CartModule.Application.Services;
using CartModule.Domain.Entities;
using CartModule.Domain.Exceptions;
using CommonModule.BusinessExceptions;
using CommonModule.Domain.Exceptions;
using MediatR;

namespace CartModule.Application.UseCases.Carts.UpdateCartItem;

internal sealed class UpdateCartItem_UCHandler : IRequestHandler<UpdateCartItem_UCInput, UpdateCartItem_UCOutput>
{
    private readonly ICartRepository _carts;
    private readonly IExecuteAtomically _transaction;

    public UpdateCartItem_UCHandler(ICartRepository carts, IExecuteAtomically transaction)
    {
        _carts = carts;
        _transaction = transaction;
    }

    public Task<UpdateCartItem_UCOutput> Handle(UpdateCartItem_UCInput input, CancellationToken cancellationToken)
    {
        return _transaction.ExecuteAtomicallyAsync(async () =>
        {
            Cart cart = await _carts.GetByUserIdForUpdateAsync(input.UserId, cancellationToken)
                ?? throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.CartNotFound);

            CartItem item;
            bool changed;
            try
            {
                (item, changed) = cart.UpdateItemQuantity(input.ProductId, input.Quantity, DateTimeOffset.UtcNow);
            }
            catch (DomainException exception) when (exception.Code == DomainExceptionCode.ItemNotFound)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.ItemNotFound);
            }

            if (changed)
            {
                bool updated = await _carts.UpdateItemAsync(item, cancellationToken);
                if (!updated)
                {
                    throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.ItemNotFound);
                }
            }

            return new UpdateCartItem_UCOutput(item.CartId, item.ProductId, item.Quantity);
        }, cancellationToken);
    }
}
