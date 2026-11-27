using MediatR;
namespace AuthModule.Application.UseCases.RefreshToken;
internal sealed record RefreshToken_UCInput(string RefreshToken) : IRequest<RefreshToken_UCOutput>;
