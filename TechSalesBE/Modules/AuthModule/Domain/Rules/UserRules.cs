using AuthModule.Domain.Constants;
using AuthModule.Domain.Enums;
using AuthModule.Domain.Exceptions;
using CommonModule.Domain.Exceptions;

namespace AuthModule.Domain.Rules;

internal static class UserRules
{
    internal static void EnsureValidCreation(string email, string passwordHash)
    {
        if (!AuthRules.ValidEmail(email))
        {
            throw new DomainException(DomainExceptionCode.INVALID_EMAIL, DomainExceptionMessage.EmailInvalid);
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PASSWORD, DomainExceptionMessage.PasswordInvalid);
        }
    }

    internal static void EnsureActive(UserStatus status)
    {
        if (status != UserStatus.ACTIVE)
        {
            throw new DomainException(DomainExceptionCode.INVALID_STATE, DomainExceptionMessage.AccountInactive);
        }
    }

    internal static void EnsurePending(UserStatus status)
    {
        if (status != UserStatus.PENDING)
        {
            throw new DomainException(DomainExceptionCode.INVALID_STATE, DomainExceptionMessage.AccountNotPending);
        }
    }

    internal static void EnsureValidPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PASSWORD, DomainExceptionMessage.PasswordHashInvalid);
        }
    }

    internal static void EnsureCanLock(UserStatus status, DateTimeOffset? until, DateTimeOffset now)
    {
        if (status != UserStatus.ACTIVE && status != UserStatus.BLOCKED)
        {
            throw new DomainException(DomainExceptionCode.INVALID_STATE, DomainExceptionMessage.VerifiedAccountRequired);
        }

        if (until.HasValue && until.Value <= now)
        {
            throw new DomainException(DomainExceptionCode.INVALID_STATE, DomainExceptionMessage.LockExpiryInPast);
        }
    }

    internal static void EnsureBlocked(UserStatus status)
    {
        if (status != UserStatus.BLOCKED)
        {
            throw new DomainException(DomainExceptionCode.INVALID_STATE, DomainExceptionMessage.AccountNotLocked);
        }
    }

    internal static bool CanReleaseExpiredLock(UserStatus status, DateTimeOffset? lockedUntil, DateTimeOffset now)
    {
        return status == UserStatus.BLOCKED && lockedUntil.HasValue && lockedUntil.Value <= now;
    }

    internal static bool ShouldResetFailedAttempts(DateTimeOffset? lastFailure, DateTimeOffset now)
    {
        if (!lastFailure.HasValue)
        {
            return true;
        }

        return lastFailure.Value.AddMinutes(AuthLimits.FailedLoginWindowMinutes) <= now;
    }

    internal static bool ShouldLockAfterFailedAttempts(int failedAttempts)
    {
        return failedAttempts >= AuthLimits.MaxFailedLoginAttempts;
    }

    internal static DateTimeOffset GetFailedLoginLockExpiry(DateTimeOffset now)
    {
        return now.AddMinutes(AuthLimits.AccountLockMinutes);
    }
}
