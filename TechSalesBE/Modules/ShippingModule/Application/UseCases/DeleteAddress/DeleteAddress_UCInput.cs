using MediatR;

namespace ShippingModule.Application.UseCases.DeleteAddress;

internal sealed record DeleteAddress_UCInput(Guid ActorId, Guid AddressId) : IRequest<DeleteAddress_UCOutput>;
