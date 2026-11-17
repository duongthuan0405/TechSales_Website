using Microsoft.AspNetCore.Authorization;

namespace AuthModule.Presentation.Authorization;

internal sealed class AccountManagementRequirement : IAuthorizationRequirement
{
    public const string PolicyName = "Auth.AccountManagement";
}
