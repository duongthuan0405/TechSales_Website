using AuthModule.Domain.Exceptions;
using System.Diagnostics.CodeAnalysis;
using AuthModule.Domain.Constants;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using CommonModule.Domain.Exceptions;

namespace AuthModule.Domain.Rules;

internal static class AuthWorkflowRules
{
    internal static void EnsureCanRegister(User? user)
    {
        if (user != null && user.Status != UserStatus.PENDING)
        {
            throw new DomainException(DomainExceptionCode.RULE_EMAIL_ALREADY_REGISTERED, DomainExceptionMessage.EmailAlreadyRegistered);
        }
    }

    internal static void EnsureCurrentPasswordMatches(bool passwordMatches)
    {
        if (!passwordMatches)
        {
            throw new DomainException(DomainExceptionCode.RULE_CURRENT_PASSWORD_INCORRECT, DomainExceptionMessage.CurrentPasswordIncorrect);
        }
    }

    internal static void EnsurePasswordChanged(bool passwordMatches)
    {
        if (passwordMatches)
        {
            throw new DomainException(DomainExceptionCode.RULE_PASSWORD_UNCHANGED, DomainExceptionMessage.PasswordUnchanged);
        }
    }

    internal static void EnsureRolesKnown(int selectedCount, int requestedCount)
    {
        if (selectedCount != requestedCount)
        {
            throw new DomainException(DomainExceptionCode.RULE_ROLE_UNKNOWN, DomainExceptionMessage.RoleUnknown);
        }
    }

    internal static void EnsureRolePermissionsEditable(Role role)
    {
        if (role.Name == AccountRoleNames.TECHNICAL_ADMIN)
        {
            throw new DomainException(DomainExceptionCode.RULE_TECHNICAL_ADMIN_PERMISSIONS_PROTECTED, DomainExceptionMessage.TechnicalAdminPermissionsProtected);
        }
    }

    internal static void EnsurePermissionsKnown(bool allExist)
    {
        if (!allExist)
        {
            throw new DomainException(DomainExceptionCode.RULE_PERMISSION_UNKNOWN, DomainExceptionMessage.PermissionUnknown);
        }
    }

    internal static void EnsureEmailAvailable(User? existing, Guid? currentUserId)
    {
        if (existing != null && existing.Id != currentUserId)
        {
            throw new DomainException(DomainExceptionCode.RULE_EMAIL_ALREADY_REGISTERED, DomainExceptionMessage.EmailAlreadyRegistered);
        }
    }

    internal static bool CanResetPassword([NotNullWhen(true)] User? user)
    {
        return user != null && (user.Status == UserStatus.ACTIVE || user.Status == UserStatus.BLOCKED);
    }

    internal static User RequireResetEligibleUser(User? user)
    {
        // Preserve the existing reset eligibility and its non-enumerating error.
        if (user == null || user.Status == UserStatus.PENDING || user.Status == UserStatus.INACTIVE)
        {
            throw new DomainException(DomainExceptionCode.RULE_OTP_INVALID, DomainExceptionMessage.OtpInvalid);
        }

        return user;
    }

    internal static bool IsBlocked(User user)
    {
        return user.Status == UserStatus.BLOCKED;
    }

    internal static bool CanLogin(User user)
    {
        return user.Status == UserStatus.ACTIVE;
    }
}
