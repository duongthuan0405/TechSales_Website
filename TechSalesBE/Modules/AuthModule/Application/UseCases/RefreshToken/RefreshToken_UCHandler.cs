using AuthModule.Application.Exceptions;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Enums;
using CommonModule.BusinessExceptions;
using MediatR;

namespace AuthModule.Application.UseCases.RefreshToken;

internal sealed class RefreshToken_UCHandler(IUserRepository users, IRefreshSessionRepository sessions,
    IRefreshTokenService refreshTokens, IAccessTokenService accessTokens, IExecuteAtomically transaction)
    : IRequestHandler<RefreshToken_UCInput, RefreshToken_UCOutput>
{
    public async Task<RefreshToken_UCOutput> Handle(RefreshToken_UCInput input, CancellationToken ct)
    {
        // Return the failure after commit so that reuse detection persists revocations.
        var output = await transaction.ExecuteAtomicallyAsync<RefreshToken_UCOutput?>(async () =>
        {
            var hash = refreshTokens.Hash(input.RefreshToken);
            var session = await sessions.FindAsync(hash, ct);
            if (session == null) return null;
            await users.LockUsersAsync([session.UserId], ct);
            session = await sessions.FindAsync(hash, ct);
            if (session == null) return null;
            var now = DateTimeOffset.UtcNow;
            if (session.RevokedAt.HasValue)
            {
                await sessions.RevokeFamilyAsync(session.FamilyId, now, ct);
                return null;
            }
            var user = await users.GetByIdAsync(session.UserId, ct);
            if (user == null || user.Status != UserStatus.ACTIVE)
            {
                await sessions.RevokeUserAsync(session.UserId, now, ct);
                return null;
            }
            if (session.ExpiresAt <= now)
            {
                await sessions.RevokeAsync(session.Id, now, ct);
                return null;
            }
            await sessions.RevokeAsync(session.Id, now, ct);
            var next = refreshTokens.Issue(user.Id, session.FamilyId, session.ExpiresAt);
            await sessions.AddAsync(next.Session, ct);
            var access = accessTokens.Issue(user);
            return new RefreshToken_UCOutput(access.Token, access.ExpiresAt, next.Token, next.Session.ExpiresAt);
        }, ct);
        return output ?? throw new BusinessException(BusinessExceptionCode.UNAUTHORIZED, BusinessExceptionMessage.RefreshTokenInvalid);
    }
}
