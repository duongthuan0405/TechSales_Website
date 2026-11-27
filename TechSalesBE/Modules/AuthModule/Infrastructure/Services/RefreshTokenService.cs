using System.Security.Cryptography;
using System.Text;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace AuthModule.Infrastructure.Services;

internal sealed class RefreshTokenService(IConfiguration configuration) : IRefreshTokenService
{
    public string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public IssuedRefreshToken Issue(Guid userId, Guid? familyId = null, DateTimeOffset? expiresAt = null)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        var now = DateTimeOffset.UtcNow;
        int days = Math.Clamp(configuration.GetValue<int?>("AUTH:RefreshToken:DurationInDays") ?? 7, 1, 30);
        return new IssuedRefreshToken(token, new RefreshSession
        {
            UserId = userId, FamilyId = familyId ?? Guid.NewGuid(), TokenHash = Hash(token),
            CreatedAt = now, ExpiresAt = expiresAt ?? now.AddDays(days),
        });
    }
}
