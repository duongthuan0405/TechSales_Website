using MediatR;
namespace AuthModule.Application.UseCases.CreateUser;
internal sealed record CreateUser_UCInput(Guid ActorId, string Email, string Password, string[] Roles) : IRequest<CreateUser_UCOutput>;
