using AuthModule.Application.Constants;
using AuthModule.Application.Repositories;
using AuthModule.Domain.Entities;
using CommonModule.BusinessExceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace AuthModule.Presentation.Authorization;
internal sealed class AccountManagementAuthorizationHandler : AuthorizationHandler<AccountManagementRequirement, AccountManagementResource>
{
    private readonly IUserRepository _users;
    private readonly IHttpContextAccessor _accessor;

    public AccountManagementAuthorizationHandler(IUserRepository users, IHttpContextAccessor accessor)
    {
        _users = users;
        _accessor = accessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AccountManagementRequirement requirement, AccountManagementResource resource)
    {
        if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
        {
            return;
        }

        if (context.User.IsInRole(UserRoleName.TECHNICAL_ADMIN))
        {
            context.Succeed(requirement);
            return;
        }

        if (!resource.TargetUserId.HasValue)
        {
            if (!context.User.IsInRole(UserRoleName.BUSINESS_ADMIN) || resource.RequestedRoles == null)
            {
                return;
            }

            foreach (string roleName in resource.RequestedRoles)
            {
                if (roleName != UserRoleName.STAFF && roleName != UserRoleName.BUSINESS_ADMIN)
                {
                    return;
                }
            }

            context.Succeed(requirement);
            return;
        }

        CancellationToken cancellationToken = CancellationToken.None;
        if (_accessor.HttpContext != null)
        {
            cancellationToken = _accessor.HttpContext.RequestAborted;
        }

        User? target = await _users.GetByIdAsync(resource.TargetUserId.Value, cancellationToken);
        if (target == null)
        {
            throw new BusinessException(BusinessExceptionCode.NOT_FOUND, "User not found.");
        }

        if (context.User.IsInRole(UserRoleName.BUSINESS_ADMIN))
        {
            foreach (Role role in target.Roles)
            {
                if (role.Name != UserRoleName.CUSTOMER && role.Name != UserRoleName.STAFF)
                {
                    return;
                }
            }

            context.Succeed(requirement);
            return;
        }

        if (context.User.IsInRole(UserRoleName.STAFF) && target.Roles.Count > 0)
        {
            foreach (Role role in target.Roles)
            {
                if (role.Name != UserRoleName.CUSTOMER)
                {
                    return;
                }
            }

            context.Succeed(requirement);
        }
    }
}
