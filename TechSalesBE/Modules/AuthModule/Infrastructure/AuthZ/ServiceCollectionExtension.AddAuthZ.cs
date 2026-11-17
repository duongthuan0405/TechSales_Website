using AuthModule.Infrastructure.AuthZ;
using AuthModule.Presentation.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AuthModule.Infrastructure.AuthZ;

internal static partial class ServiceCollectionExtension
{
    internal static IServiceCollection AddAuthZ(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, AccountManagementAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AccountManagementRequirement.PolicyName, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new AccountManagementRequirement());
            });
        });
        return services;
    }
}
