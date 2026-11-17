using MediatR;
namespace AuthModule.Application.UseCases.UnlockUser;
internal sealed record UnlockUser_UCInput(Guid ActorId, Guid UserId) : IRequest<UnlockUser_UCOutput>;
