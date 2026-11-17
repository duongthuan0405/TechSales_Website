using AuthModule.Application.Repositories;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using CommonModule.BusinessExceptions;

namespace AuthModule.Application.Services;
internal interface IAuthOtpManager
{
    Task<string> CreateOtpAsync(Guid userId, OtpType type, CancellationToken ct);
    Task ConsumeOtpAsync(Guid userId, OtpType type, string value, CancellationToken ct);
}
internal sealed class AuthOtpManager : IAuthOtpManager
{
    private readonly IUserOtpRepository _tokens;
    private readonly IOtpService _otp;

    public AuthOtpManager(IUserOtpRepository tokens, IOtpService otp)
    {
        _tokens = tokens;
        _otp = otp;
    }

    public async Task<string> CreateOtpAsync(Guid userId, OtpType type, CancellationToken ct)
    {
        var generated = _otp.GenerateOtp(type, userId);
        await _tokens.UpsertAsync(new UserOtp(userId, generated.HashedOtp, type, generated.ExpiredAt), ct);
        return generated.Otp;
    }

    public async Task ConsumeOtpAsync(Guid userId, OtpType type, string value, CancellationToken ct)
    {
        const string invalidOtpMessage = "Verification code is invalid or expired.";
        UserOtp? token = await _tokens.GetByUserIdAndTypeAsync(userId, type, ct);
        if (token == null)
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, invalidOtpMessage);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (token.UsedAt.HasValue || token.ExpiredAt <= now)
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, invalidOtpMessage);
        }

        bool isValidOtp = _otp.IsValidOtp(value, type, userId, token.Otp);
        if (!isValidOtp)
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, invalidOtpMessage);
        }

        token.UsedAt = now;
        await _tokens.UpsertAsync(token, ct);
    }
}
