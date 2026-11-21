using MediatR;

namespace CartModule.Application.UseCases.Carts.RemoveCartItem;

internal sealed record RemoveCartItem_UCInput(Guid UserId, Guid ProductId) : IRequest<RemoveCartItem_UCOutput>;
