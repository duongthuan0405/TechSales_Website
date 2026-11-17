using AuthModule.Domain.Enums;

namespace AuthModule.Infrastructure.Persistence.Models;

internal sealed class UserDbModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTimeOffset? LastLoginFailedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }

    public ICollection<UserOtpDbModel> Otps { get; set; } = new HashSet<UserOtpDbModel>();
    public UserProfileDbModel? Profile { get; set; }
    public ICollection<UserRoleDbModel> UserRoles { get; set; } = new HashSet<UserRoleDbModel>();
}
