using AuthModule.Domain.Constants;
using AuthModule.Application.Services;
using AuthModule.Domain.Rules;
using AuthModule.Domain.Exceptions;
using CommonModule.Domain.Exceptions;

namespace AuthModule.Infrastructure.Services;
public sealed class PasswordHasher : IPasswordHasher
{
    private static readonly Lazy<string> DummyHash = new(() => BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N"), workFactor: 12));
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string? hashedPassword)
    {
        if (System.Text.Encoding.UTF8.GetByteCount(password) > AuthLimits.PasswordMaxUtf8Bytes)
        {
            return false;
        }

        try
        {
            var matches = BCrypt.Net.BCrypt.Verify(password, hashedPassword ?? DummyHash.Value);
            return hashedPassword is not null && matches;
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}
