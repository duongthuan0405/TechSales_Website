using MediatR;

namespace ShippingModule.Application.UseCases.UpdateAddress;

internal sealed record UpdateAddress_UCInput(Guid ActorId, Guid AddressId, string WardCode, string Detail) : IRequest<UpdateAddress_UCOutput>;
