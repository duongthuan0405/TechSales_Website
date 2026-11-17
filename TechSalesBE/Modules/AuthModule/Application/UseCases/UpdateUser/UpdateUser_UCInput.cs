using MediatR;
namespace AuthModule.Application.UseCases.UpdateUser;
internal sealed record UpdateUser_UCInput(Guid ActorId, Guid UserId, string Email) : IRequest<UpdateUser_UCOutput>;
