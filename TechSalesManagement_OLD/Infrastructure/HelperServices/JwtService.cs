using System;
using System.Collections.Generic;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TechSalesManagement.Application.Common.Configurations;
using TechSalesManagement.Application.HelperServices;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Infrastructure.HelperServices;

public class JwtService : IJwtService
{
    private readonly JwtCO _jwtCO;

    public JwtService(IOptions<JwtCO> jwtOptions)
    {
        _jwtCO = jwtOptions.Value;
    }

    public string GenerateToken(User user, IEnumerable<Role> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.id.ToString())
        };

        // Thêm role vào claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.name));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtCO.secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtCO.issuer,
            audience: _jwtCO.audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtCO.durationInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Guid? ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtCO.secretKey);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtCO.issuer,
                ValidateAudience = true,
                ValidAudience = _jwtCO.audience,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = jwtToken.Claims.First(x => x.Type == "id").Value;

            return Guid.Parse(userId);
        }
        catch
        {
            return null;
        }
    }
}
