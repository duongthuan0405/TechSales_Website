using Microsoft.AspNetCore.Authorization;

namespace AuthModule.Infrastructure.AuthZ;

internal class HasPermissionRequirement : IAuthorizationRequirement
{
    private readonly string _permission = "";
    
    public HasPermissionRequirement(string permission)
    {
        _permission = permission;
    }

    public string Permission => _permission;
}
