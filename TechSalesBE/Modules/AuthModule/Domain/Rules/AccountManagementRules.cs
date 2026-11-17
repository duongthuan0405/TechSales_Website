using AuthModule.Domain.Exceptions;
using AuthModule.Domain.Constants;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using CommonModule.Domain.Exceptions;

namespace AuthModule.Domain.Rules;

internal static class AccountManagementRules
{
    public static void EnsureCanCreate(User actor, IEnumerable<string> requestedRoles)
    {
        EnsureActiveActor(actor);
        if (HasRole(actor, AccountRoleNames.TECHNICAL_ADMIN))
        {
            return;
        }

        foreach (string roleName in requestedRoles)
        {
            if (roleName == AccountRoleNames.TECHNICAL_ADMIN)
            {
                throw new DomainException(DomainExceptionCode.RULE_CANNOT_CREATE_TECHNICAL_ADMIN, DomainExceptionMessage.CannotCreateTechnicalAdmin);
            }
        }
    }

    public static void EnsureCanManage(User actor, User target)
    {
        EnsureActiveActor(actor);
        EnsureDifferentAccount(actor.Id, target.Id);
        if (HasRole(actor, AccountRoleNames.TECHNICAL_ADMIN))
        {
            return;
        }

        if (HasRole(actor, AccountRoleNames.BUSINESS_ADMIN))
        {
            foreach (Role role in target.Roles)
            {
                if (role.Name != AccountRoleNames.CUSTOMER && role.Name != AccountRoleNames.STAFF)
                {
                    throw new DomainException(DomainExceptionCode.RULE_CANNOT_MANAGE_ADMIN, DomainExceptionMessage.CannotManageAdmin);
                }
            }
            return;
        }

        if (HasRole(actor, AccountRoleNames.STAFF) && target.Roles.Count > 0)
        {
            foreach (Role role in target.Roles)
            {
                if (role.Name != AccountRoleNames.CUSTOMER)
                {
                    throw new DomainException(DomainExceptionCode.RULE_STAFF_CAN_ONLY_MANAGE_CUSTOMERS, DomainExceptionMessage.StaffCanOnlyManageCustomers);
                }
            }
            return;
        }

        throw new DomainException(DomainExceptionCode.RULE_CANNOT_MANAGE_ACCOUNT, DomainExceptionMessage.CannotManageAccount);
    }

    public static void EnsureCanAssignRoles(User actor, Guid targetUserId)
    {
        EnsureActiveActor(actor);
        EnsureDifferentAccount(actor.Id, targetUserId);
        if (!HasRole(actor, AccountRoleNames.TECHNICAL_ADMIN))
        {
            throw new DomainException(DomainExceptionCode.RULE_TECHNICAL_ADMIN_REQUIRED, DomainExceptionMessage.TechnicalAdminRequired);
        }
    }

    public static List<string> GetVisibleStaffRoles(User actor)
    {
        EnsureActiveActor(actor);
        List<string> roleNames = new List<string> { AccountRoleNames.STAFF };
        if (HasRole(actor, AccountRoleNames.TECHNICAL_ADMIN))
        {
            roleNames.Add(AccountRoleNames.BUSINESS_ADMIN);
            roleNames.Add(AccountRoleNames.TECHNICAL_ADMIN);
        }
        return roleNames;
    }

    private static void EnsureDifferentAccount(Guid actorId, Guid targetUserId)
    {
        if (actorId == targetUserId)
        {
            throw new DomainException(DomainExceptionCode.RULE_CANNOT_MANAGE_SELF, DomainExceptionMessage.CannotManageSelf);
        }
    }

    private static void EnsureActiveActor(User actor)
    {
        if (actor.Status != UserStatus.ACTIVE)
        {
            throw new DomainException(DomainExceptionCode.RULE_ACTIVE_ACTOR_REQUIRED, DomainExceptionMessage.ActiveActorRequired);
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
