namespace AuthModule.Application.Services;

public class OtpResult 
{
    public string Otp {get; set;} = string.Empty;
    public DateTimeOffset ExpiredAt {get; set;}
}

public interface IOtpService
{
    OtpResult GenerateOtp();
}