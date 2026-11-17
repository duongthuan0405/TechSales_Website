using MediatR;
namespace AuthModule.Application.UseCases.Login;
internal sealed record Login_UCInput(string Email, string Password) : IRequest<Login_UCOutput>;
