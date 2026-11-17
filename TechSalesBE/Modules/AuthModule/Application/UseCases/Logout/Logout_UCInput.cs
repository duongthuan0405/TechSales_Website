using MediatR;
namespace AuthModule.Application.UseCases.Logout;
internal sealed record Logout_UCInput(Guid ActorId) : IRequest<Logout_UCOutput>;
