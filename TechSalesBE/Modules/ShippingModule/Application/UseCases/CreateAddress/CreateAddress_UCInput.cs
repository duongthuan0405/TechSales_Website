using MediatR;

namespace ShippingModule.Application.UseCases.CreateAddress;

internal sealed record CreateAddress_UCInput(Guid ActorId, string WardCode, string Detail) : IRequest<CreateAddress_UCOutput>;
