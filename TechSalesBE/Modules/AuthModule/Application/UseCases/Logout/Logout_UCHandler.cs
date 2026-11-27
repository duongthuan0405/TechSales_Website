using AuthModule.Application.Messages;
using MediatR;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;

namespace AuthModule.Application.UseCases.Logout;

internal sealed class Logout_UCHandler(IUserRepository users, IRefreshSessionRepository sessions,
    IRefreshTokenService tokens, IExecuteAtomically transaction) : IRequestHandler<Logout_UCInput, Logout_UCOutput>
{
    public Task<Logout_UCOutput> Handle(Logout_UCInput input, CancellationToken cancellationToken)
    {
        return transaction.ExecuteAtomicallyAsync(async () =>
        {
            var session = await sessions.FindAsync(tokens.Hash(input.RefreshToken), cancellationToken);
            if (session != null)
            {
                await users.LockUsersAsync([session.UserId], cancellationToken);
                await sessions.RevokeFamilyAsync(session.FamilyId, DateTimeOffset.UtcNow, cancellationToken);
            }
            else if (input.ActorId != Guid.Empty)
            {
                await users.LockUsersAsync([input.ActorId], cancellationToken);
                await sessions.RevokeUserAsync(input.ActorId, DateTimeOffset.UtcNow, cancellationToken);
            }
            return new Logout_UCOutput(ResponseMessage.SuccessLogout);
        }, cancellationToken);
    }
}
