using AuthModule.Infrastructure.Services;

namespace AuthModule.Application.Services;

public interface IOtpService
{
    OtpResult GenerateOtp();
}