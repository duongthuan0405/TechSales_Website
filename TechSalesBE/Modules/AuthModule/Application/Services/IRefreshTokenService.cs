using AuthModule.Domain.Entities;

namespace AuthModule.Application.Services;

internal sealed record IssuedRefreshToken(string Token, RefreshSession Session);

internal interface IRefreshTokenService
{
    string Hash(string token);
    IssuedRefreshToken Issue(Guid userId, Guid? familyId = null, DateTimeOffset? expiresAt = null);
}
