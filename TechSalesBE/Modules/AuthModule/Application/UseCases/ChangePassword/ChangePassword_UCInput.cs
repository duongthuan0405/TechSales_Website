using MediatR;
namespace AuthModule.Application.UseCases.ChangePassword;
internal sealed record ChangePassword_UCInput(Guid ActorId, string CurrentPassword, string NewPassword, string ConfirmPassword) : IRequest<ChangePassword_UCOutput>;
