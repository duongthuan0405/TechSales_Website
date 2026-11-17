using MediatR;
namespace AuthModule.Application.UseCases.ForgotPassword;
internal sealed record ForgotPassword_UCInput(string Email) : IRequest<ForgotPassword_UCOutput>;
