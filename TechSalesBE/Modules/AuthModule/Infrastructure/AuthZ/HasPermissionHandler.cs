using AuthModule.Application.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace AuthModule.Infrastructure.AuthZ;

internal class HasPermissionHandler : AuthorizationHandler<HasPermissionRequirement>
{
    private readonly IPermissionRepository _permissionRepository;
    public HasPermissionHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HasPermissionRequirement requirement)
    {
        if (await _permissionRepository.CheckUserHasPermissionAsync(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
