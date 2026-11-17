using System.Security.Claims;
using AuthModule.Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace AuthModule.Infrastructure.AuthZ;
internal sealed class HasPermissionHandler : AuthorizationHandler<HasPermissionRequirement>
{
    private readonly IPermissionRepository _permissions;
    private readonly IHttpContextAccessor _accessor;

    public HasPermissionHandler(IPermissionRepository permissions, IHttpContextAccessor accessor)
    {
        _permissions = permissions;
        _accessor = accessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HasPermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true && Guid.TryParse(context.User.FindFirstValue("sub"), out var userId) && await _permissions.CheckUserHasPermissionAsync(userId, requirement.Permission, _accessor.HttpContext?.RequestAborted ?? default))
        {
            context.Succeed(requirement);
        }
    }
}
