using MediatR;

namespace ShippingModule.Application.UseCases.SetDefaultAddress;

internal sealed record SetDefaultAddress_UCInput(Guid ActorId, Guid AddressId) : IRequest<SetDefaultAddress_UCOutput>;
