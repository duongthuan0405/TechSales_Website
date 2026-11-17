using AuthModule.Domain.Exceptions;
using AuthModule.Domain.Entities;
using CommonModule.Domain.Exceptions;

namespace AuthModule.Domain.Rules;

internal static class OtpRules
{

    internal static UserOtp RequireUsable(UserOtp? token, DateTimeOffset now)
    {
        if (token == null || token.UsedAt.HasValue || token.ExpiredAt <= now)
        {
            throw new DomainException(DomainExceptionCode.RULE_OTP_INVALID, DomainExceptionMessage.OtpInvalid);
        }

        return token;
    }

    internal static void EnsureMatches(bool isValidOtp)
    {
        if (!isValidOtp)
        {
            throw new DomainException(DomainExceptionCode.RULE_OTP_INVALID, DomainExceptionMessage.OtpInvalid);
        }
    }
}
