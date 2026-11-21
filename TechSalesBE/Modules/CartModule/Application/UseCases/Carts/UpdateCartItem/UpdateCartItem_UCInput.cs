using MediatR;

namespace CartModule.Application.UseCases.Carts.UpdateCartItem;

internal sealed record UpdateCartItem_UCInput(Guid UserId, Guid ProductId, int Quantity)
    : IRequest<UpdateCartItem_UCOutput>;
