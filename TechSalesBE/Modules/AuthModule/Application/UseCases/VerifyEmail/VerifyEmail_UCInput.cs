using MediatR;
namespace AuthModule.Application.UseCases.VerifyEmail;
internal sealed record VerifyEmail_UCInput(string Email, string Token) : IRequest<VerifyEmail_UCOutput>;
