using MediatR;

namespace ShippingModule.Application.UseCases.GetAddresses;

internal sealed record GetAddresses_UCInput(Guid ActorId) : IRequest<GetAddresses_UCOutput>;
