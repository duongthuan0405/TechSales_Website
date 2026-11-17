using MediatR;
namespace AuthModule.Application.UseCases.LockUser;
internal sealed record LockUser_UCInput(Guid ActorId, Guid UserId, DateTimeOffset? Until) : IRequest<LockUser_UCOutput>;
