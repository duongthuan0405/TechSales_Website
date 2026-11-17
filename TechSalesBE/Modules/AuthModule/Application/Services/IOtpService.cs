using AuthModule.Domain.Enums;

namespace AuthModule.Application.Services;

internal class OtpResult 
{
    public string Otp {get; set;} = string.Empty;
    public string HashedOtp {get; set;} = string.Empty;
    public DateTimeOffset ExpiredAt {get; set;}
}

internal interface IOtpService
{
    OtpResult GenerateOtp(OtpType type, Guid ownerId);
    bool IsValidOtp(string otp, OtpType type, Guid ownerId, string hashedOtp);
}
