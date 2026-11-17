using MediatR;
namespace AuthModule.Application.UseCases.ResetPassword;
internal sealed record ResetPassword_UCInput(string Email, string Token, string NewPassword, string ConfirmPassword) : IRequest<ResetPassword_UCOutput>;
