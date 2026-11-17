using AuthModule.Application.Messages;
using MediatR;

namespace AuthModule.Application.UseCases.Logout;

internal sealed class Logout_UCHandler : IRequestHandler<Logout_UCInput, Logout_UCOutput>
{
    public Task<Logout_UCOutput> Handle(Logout_UCInput input, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Logout_UCOutput output = new Logout_UCOutput(ResponseMessage.SuccessLogout);
        return Task.FromResult(output);
    }
}
