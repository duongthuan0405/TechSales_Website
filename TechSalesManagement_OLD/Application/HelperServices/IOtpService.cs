using System;

namespace TechSalesManagement.Application.HelperServices;

public record OtpResult(string otp, DateTimeOffset expiredAt);

public interface IOtpService
{
    OtpResult GenerateOtp();
    bool ValidateOtp(string otp, string storedOtp);
}
