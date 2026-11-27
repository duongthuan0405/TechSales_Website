using MediatR;

namespace AuthModule.Application.UseCases.DeactivateUser;

internal sealed record DeactivateUser_UCInput(Guid ActorId, Guid UserId) : IRequest<DeactivateUser_UCOutput>;
