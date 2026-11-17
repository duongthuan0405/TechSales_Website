using AuthModule.Domain.Constants;
using AuthModule.Domain.Enums;
using AuthModule.Domain.Exceptions;
using AuthModule.Domain.Rules;
using CommonModule.Domain.Exceptions;
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
        if (!AuthRules.ValidEmail(email))
        {
            throw new DomainException(DomainExceptionCode.INVALID_EMAIL, "Invalid email address.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PASSWORD, "Invalid password hash.");
        }

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
        if (Status != UserStatus.BLOCKED || !LockedUntil.HasValue)
        {
            return;
        }

        DateTimeOffset lockExpiresAt = LockedUntil.Value;
        if (lockExpiresAt > now)
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
        if (LastLoginFailedAt.HasValue)
        {
            DateTimeOffset lastLoginFailedAt = LastLoginFailedAt.Value;
            DateTimeOffset failedLoginWindowEndsAt = lastLoginFailedAt.AddMinutes(AuthLimits.FailedLoginWindowMinutes);

            if (failedLoginWindowEndsAt <= now)
            {
                FailedLoginAttempts = 0;
            }
        }
        else
        {
            FailedLoginAttempts = 0;
        }

        FailedLoginAttempts++;
        LastLoginFailedAt = now;
        UpdatedAt = now;

        if (FailedLoginAttempts >= AuthLimits.MaxFailedLoginAttempts)
        {
            DateTimeOffset lockExpiresAt = now.AddMinutes(AuthLimits.AccountLockMinutes);
            LockAccount(lockExpiresAt, now);
        }
    }

    internal void RecordSuccessfulLogin(DateTimeOffset now)
    {
        if (Status != UserStatus.ACTIVE)
        {
            throw new DomainException(DomainExceptionCode.INVALID_STATE, "Account is not active.");
        }

        FailedLoginAttempts = 0;
        LastLoginFailedAt = null;
        LockedUntil = null;
        UpdatedAt = now;
    }

    internal void VerifyEmail(DateTimeOffset now)
    {
        if (Status != UserStatus.PENDING)
        {
            throw new DomainException(DomainExceptionCode.INVALID_STATE, "Account is not pending verification.");
        }

        Status = UserStatus.ACTIVE;
        UpdatedAt = now;
    }

    internal void ChangePassword(string passwordHash, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PASSWORD, "Invalid password hash.");
        }

        Password = passwordHash;
        FailedLoginAttempts = 0;
        LastLoginFailedAt = null;
        UpdatedAt = now;
        // A password reset must not remove a manual administrative lock.
        ReleaseExpiredLock(now);
    }

    internal void LockAccount(DateTimeOffset? until, DateTimeOffset now)
    {
        if (Status != UserStatus.ACTIVE && Status != UserStatus.BLOCKED)
        {
            throw new DomainException(DomainExceptionCode.INVALID_STATE, "Only verified accounts can be locked.");
        }

        if (until.HasValue && until.Value <= now)
        {
            throw new DomainException(DomainExceptionCode.INVALID_STATE, "Lock expiry must be in the future.");
        }

        Status = UserStatus.BLOCKED;
        LockedUntil = until;
        UpdatedAt = now;
    }

    internal void UnlockAccount(DateTimeOffset now)
    {
        if (Status != UserStatus.BLOCKED)
        {
            throw new DomainException(DomainExceptionCode.INVALID_STATE, "Account is not locked.");
        }

        Status = UserStatus.ACTIVE;
        FailedLoginAttempts = 0;
        LastLoginFailedAt = null;
        LockedUntil = null;
        UpdatedAt = now;
    }
}
