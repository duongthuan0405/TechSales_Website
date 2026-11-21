using MediatR;

namespace CartModule.Application.UseCases.Carts.AddCartItem;

internal sealed record AddCartItem_UCInput(Guid UserId, Guid ProductId, int Quantity) : IRequest<AddCartItem_UCOutput>;
