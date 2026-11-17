using AuthModule.Domain.Enums;

namespace AuthModule.Domain.Entities;

internal class UserOtp
{
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private Guid _id = Guid.Empty;
    private DateTimeOffset? _updatedAt =  null; 
    private Guid _userId = Guid.Empty;
    private string _otp = string.Empty;
    private OtpType _type;
    private DateTimeOffset _expiredAt = DateTimeOffset.UtcNow;
    private DateTimeOffset? _usedAt = null;

    internal DateTimeOffset CreatedAt { get => _createdAt; set => _createdAt = value; }
    internal Guid Id { get => _id; set => _id = value; }
    internal DateTimeOffset? UpdatedAt { get => _updatedAt; set => _updatedAt = value; }
    internal Guid UserId { get => _userId; set => _userId = value; }
    internal string Otp { get => _otp; set => _otp = value; }
    internal OtpType Type { get => _type; set => _type = value; }
    internal DateTimeOffset ExpiredAt { get => _expiredAt; set => _expiredAt = value; }
    internal DateTimeOffset? UsedAt { get => _usedAt; set => _usedAt = value; }

    internal UserOtp(Guid userId, string otp, OtpType type, DateTimeOffset expiredAt)
    {
        this.UserId = userId;
        this.Otp = otp;
        this.Type = type;
        this.ExpiredAt = expiredAt;
    }

    internal UserOtp() { }
}
