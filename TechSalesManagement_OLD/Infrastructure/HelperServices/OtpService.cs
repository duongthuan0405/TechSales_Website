using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using TechSalesManagement.Application.Common.Configurations;
using TechSalesManagement.Application.HelperServices;

namespace TechSalesManagement.Infrastructure.HelperServices;

public class OtpService : IOtpService
{
    private readonly OtpCO _config;

    public OtpService(IOptions<OtpCO> options)
    {
        _config = options.Value ?? new OtpCO();
    }

    public OtpResult GenerateOtp()
    {
        int length = _config.length > 0 ? _config.length : 6;
        int duration = _config.durationInMinutes > 0 ? _config.durationInMinutes : 5;

        const string chars = "0123456789";
        var result = new StringBuilder(length);
        
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] uintBuffer = new byte[sizeof(uint)];
            
            while (result.Length < length)
            {
                rng.GetBytes(uintBuffer);
                uint num = BitConverter.ToUInt32(uintBuffer, 0);
                int index = (int)(num % (uint)chars.Length);
                result.Append(chars[index]);
            }
        }

        string otp = result.ToString();
        DateTimeOffset expiredAt = DateTimeOffset.UtcNow.AddMinutes(duration);

        return new OtpResult(otp, expiredAt);
    }

    public bool ValidateOtp(string otp, string storedOtp)
    {
        if (string.IsNullOrWhiteSpace(otp) || string.IsNullOrWhiteSpace(storedOtp))
            return false;
            
        return otp.Equals(storedOtp, StringComparison.Ordinal);
    }
}
