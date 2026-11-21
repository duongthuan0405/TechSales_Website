using MediatR;

namespace CartModule.Application.UseCases.Carts.GetCart;

internal sealed record GetCart_UCInput(Guid UserId) : IRequest<GetCart_UCOutput>;
