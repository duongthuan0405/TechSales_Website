using AuthModule.Domain.Constants;
using System.Security.Cryptography;
using AuthModule.Application.Services;
using System.Text;
using AuthModule.Domain.Enums;
namespace AuthModule.Infrastructure.Services;
internal sealed class OtpService : IOtpService
{
    public OtpResult GenerateOtp(OtpType type, Guid ownerId)
    {
        string otp = new string(Enumerable.Range(0, AuthLimits.OtpLength)
            .Select(_ => (char)('0' + RandomNumberGenerator.GetInt32(10))).ToArray());
        string hashedOtp = Hash(NormalizeOtp(otp, ownerId, type));

        return new()
        {
            Otp = otp,
            ExpiredAt = DateTimeOffset.UtcNow.AddMinutes(AuthLimits.OtpLifetimeMinutes),
            HashedOtp = hashedOtp
        };
    }

    public bool IsValidOtp(string otp, OtpType type, Guid ownerId, string hashedOtp)
    {
        return CryptographicOperations.FixedTimeEquals(
        Encoding.UTF8.GetBytes(Hash(NormalizeOtp(otp, ownerId, type))), Encoding.UTF8.GetBytes(hashedOtp));
    }

    private string Hash(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    private string NormalizeOtp(string otp, Guid ownerId, OtpType type)
    {
        return $"{type}:{ownerId}:{otp}";
    }
}
