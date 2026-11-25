using MediatR;
namespace AuthModule.Application.UseCases.Logout;
internal sealed record Logout_UCInput(Guid ActorId, string RefreshToken = "") : IRequest<Logout_UCOutput>;
