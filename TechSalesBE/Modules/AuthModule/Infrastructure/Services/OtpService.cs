using AuthModule.Application.Services;

namespace AuthModule.Infrastructure.Services;

public class OtpResult 
{
    public string Otp {get; set;} = string.Empty;
    public DateTimeOffset ExpiredAt {get; set;}
}

public class OtpService : IOtpService
{
    public OtpResult GenerateOtp()
    {
        throw new NotImplementedException();
    }
}