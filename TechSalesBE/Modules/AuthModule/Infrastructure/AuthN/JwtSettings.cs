using AuthModule.Domain.Constants;
using Microsoft.Extensions.Configuration;

namespace AuthModule.Infrastructure.AuthN;
internal sealed class JwtSettings
{
    public byte[] Key { get; }
    public string Issuer { get; }
    public string Audience { get; }
    public int DurationInMinutes { get; }

    public JwtSettings(IConfiguration configuration)
    {
        var section = configuration.GetSection("AUTH:JWT");
        Key = Convert.FromBase64String(section["SecretKey"] ?? throw new InvalidOperationException("Configure AUTH__JWT__SecretKey."));
        if (Key.Length < 32)
        {
            throw new InvalidOperationException("AUTH__JWT__SecretKey must decode to at least 32 random bytes.");
        }

        Issuer = section["Issuer"] ?? throw new InvalidOperationException("Configure AUTH__JWT__Issuer.");
        Audience = section["Audience"] ?? throw new InvalidOperationException("Configure AUTH__JWT__Audience.");
        DurationInMinutes = section.GetValue("DurationInMinutes", AuthLimits.DefaultTokenLifetimeMinutes);
        if (string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience) || DurationInMinutes is < AuthLimits.MinTokenLifetimeMinutes or > AuthLimits.MaxTokenLifetimeMinutes)
        {
            throw new InvalidOperationException("Invalid AUTH JWT issuer, audience or lifetime.");
        }
    }
}
