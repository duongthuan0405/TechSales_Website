using AuthModule.Domain.Enums;
using AuthModule.Domain.Rules;
namespace AuthModule.Domain.Entities;
internal sealed class User
{
    internal Guid Id { get; init; } = Guid.NewGuid();
    internal string Email { get; set; } = string.Empty;
    internal string Password { get; set; } = string.Empty;
    internal UserStatus Status { get; set; } = UserStatus.PENDING;
    internal int FailedLoginAttempts { get; set; }
    internal DateTimeOffset? LastLoginFailedAt { get; set; }
    internal DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    internal DateTimeOffset? UpdatedAt { get; set; }
    internal DateTimeOffset? LockedUntil { get; set; }
    internal List<Role> Roles { get; set; } = new List<Role>();
    internal UserProfile Profile { get; set; } = new UserProfile();

    internal static User Create(string email, string passwordHash, bool verified = false)
    {
        UserRules.EnsureValidCreation(email, passwordHash);

        User user = new User
        {
            Email = AuthRules.NormalizeEmail(email),
            Password = passwordHash
        };

        if (verified)
        {
            user.Status = UserStatus.ACTIVE;
        }

        return user;
    }

    internal void ReleaseExpiredLock(DateTimeOffset now)
    {
        if (!UserRules.CanReleaseExpiredLock(Status, LockedUntil, now))
        {
            return;
        }

        Status = UserStatus.ACTIVE;
        FailedLoginAttempts = 0;
        LastLoginFailedAt = null;
        LockedUntil = null;
        UpdatedAt = now;
    }

    internal void RecordFailedLogin(DateTimeOffset now)
    {
        if (UserRules.ShouldResetFailedAttempts(LastLoginFailedAt, now))
        {
            FailedLoginAttempts = 0;
        }

        FailedLoginAttempts++;
        LastLoginFailedAt = now;
        UpdatedAt = now;

        if (UserRules.ShouldLockAfterFailedAttempts(FailedLoginAttempts))
        {
            DateTimeOffset lockExpiresAt = UserRules.GetFailedLoginLockExpiry(now);
            LockAccount(lockExpiresAt, now);
        }
    }

    internal void RecordSuccessfulLogin(DateTimeOffset now)
    {
        UserRules.EnsureActive(Status);

        FailedLoginAttempts = 0;
        LastLoginFailedAt = null;
        LockedUntil = null;
        UpdatedAt = now;
    }

    internal void VerifyEmail(DateTimeOffset now)
    {
        UserRules.EnsurePending(Status);

        Status = UserStatus.ACTIVE;
        UpdatedAt = now;
    }

    internal void ChangePassword(string passwordHash, DateTimeOffset now)
    {
        UserRules.EnsureValidPasswordHash(passwordHash);

        Password = passwordHash;
        FailedLoginAttempts = 0;
        LastLoginFailedAt = null;
        UpdatedAt = now;
        // A password reset must not remove a manual administrative lock.
        ReleaseExpiredLock(now);
    }

    internal void LockAccount(DateTimeOffset? until, DateTimeOffset now)
    {
        UserRules.EnsureCanLock(Status, until, now);

        Status = UserStatus.BLOCKED;
        LockedUntil = until;
        UpdatedAt = now;
    }

    internal void UnlockAccount(DateTimeOffset now)
    {
        UserRules.EnsureBlocked(Status);

        Status = UserStatus.ACTIVE;
        FailedLoginAttempts = 0;
        LastLoginFailedAt = null;
        LockedUntil = null;
        UpdatedAt = now;
    }
}
