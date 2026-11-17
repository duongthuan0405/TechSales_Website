using AuthModule.Domain.Enums;

namespace AuthModule.Infrastructure.Persistence.Models;

internal sealed class UserOtpDbModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Value { get; set; } = string.Empty;
    public OtpType Type { get; set; }
    public DateTimeOffset ExpiredAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public UserDbModel User { get; set; } = null!;
}
