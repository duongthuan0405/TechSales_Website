using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace AuthModule.Infrastructure.AuthN;

internal sealed class AccessTokenService : IAccessTokenService
{
    private readonly JwtSettings _settings;

    public AccessTokenService(JwtSettings settings)
    {
        _settings = settings;
    }

    public IssuedAccessToken Issue(User user)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        List<Claim> claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };
        foreach (Role role in user.Roles)
        {
            claims.Add(new Claim("role", role.Name));
        }

        DateTimeOffset expiresAt = now.AddMinutes(_settings.DurationInMinutes);
        SymmetricSecurityKey signingKey = new SymmetricSecurityKey(_settings.Key);
        SigningCredentials credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        JwtSecurityToken jwt = new JwtSecurityToken(
            _settings.Issuer, _settings.Audience, claims, now.UtcDateTime, expiresAt.UtcDateTime, credentials);
        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        string token = handler.WriteToken(jwt);
        return new IssuedAccessToken(token, expiresAt);
    }
}
