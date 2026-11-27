using MediatR;
namespace AuthModule.Application.UseCases.Register;
internal sealed record Register_UCInput(string Email, string Password, string ConfirmPassword, string FullName, string Phone) : IRequest<Register_UCOutput>;
