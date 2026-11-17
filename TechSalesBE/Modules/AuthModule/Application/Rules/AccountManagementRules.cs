using AuthModule.Application.Constants;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using CommonModule.BusinessExceptions;

namespace AuthModule.Application.Rules;

internal static class AccountManagementRules
{
    public static void EnsureCanCreate(User actor, IEnumerable<string> requestedRoles)
    {
        EnsureActiveActor(actor);
        if (HasRole(actor, UserRoleName.TECHNICAL_ADMIN))
        {
            return;
        }

        foreach (string roleName in requestedRoles)
        {
            if (roleName == UserRoleName.TECHNICAL_ADMIN)
            {
                throw new BusinessException(BusinessExceptionCode.FORBIDDEN,
                    "Business Admin can only create Staff or Business Admin accounts.");
            }
        }
    }

    public static void EnsureCanManage(User actor, User target)
    {
        EnsureActiveActor(actor);
        EnsureDifferentAccount(actor.Id, target.Id);
        if (HasRole(actor, UserRoleName.TECHNICAL_ADMIN))
        {
            return;
        }

        if (HasRole(actor, UserRoleName.BUSINESS_ADMIN))
        {
            foreach (Role role in target.Roles)
            {
                if (role.Name != UserRoleName.CUSTOMER && role.Name != UserRoleName.STAFF)
                {
                    throw new BusinessException(BusinessExceptionCode.FORBIDDEN, "You cannot manage this administrative account.");
                }
            }
            return;
        }

        if (HasRole(actor, UserRoleName.STAFF) && target.Roles.Count > 0)
        {
            foreach (Role role in target.Roles)
            {
                if (role.Name != UserRoleName.CUSTOMER)
                {
                    throw new BusinessException(BusinessExceptionCode.FORBIDDEN, "Staff can only manage Customer accounts.");
                }
            }
            return;
        }

        throw new BusinessException(BusinessExceptionCode.FORBIDDEN, "You cannot manage this account.");
    }

    public static void EnsureCanAssignRoles(User actor, Guid targetUserId)
    {
        EnsureActiveActor(actor);
        EnsureDifferentAccount(actor.Id, targetUserId);
        if (!HasRole(actor, UserRoleName.TECHNICAL_ADMIN))
        {
            throw new BusinessException(BusinessExceptionCode.FORBIDDEN, "Only Technical Admin can assign roles.");
        }
    }

    public static List<string> GetVisibleStaffRoles(User actor)
    {
        EnsureActiveActor(actor);
        List<string> roleNames = new List<string> { UserRoleName.STAFF };
        if (HasRole(actor, UserRoleName.TECHNICAL_ADMIN))
        {
            roleNames.Add(UserRoleName.BUSINESS_ADMIN);
            roleNames.Add(UserRoleName.TECHNICAL_ADMIN);
        }
        return roleNames;
    }

    private static void EnsureDifferentAccount(Guid actorId, Guid targetUserId)
    {
        if (actorId == targetUserId)
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST,
                "Use your own profile/password endpoints to manage your account.");
        }
    }

    private static void EnsureActiveActor(User actor)
    {
        if (actor.Status != UserStatus.ACTIVE)
        {
            throw new BusinessException(BusinessExceptionCode.FORBIDDEN, "Only active accounts can manage accounts.");
        }
    }

    private static bool HasRole(User user, string roleName)
    {
        foreach (Role role in user.Roles)
        {
            if (role.Name == roleName)
            {
                return true;
            }
        }
        return false;
    }
}
