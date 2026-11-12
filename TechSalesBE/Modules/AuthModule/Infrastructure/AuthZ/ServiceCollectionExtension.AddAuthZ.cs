using AuthModule.Infrastructure.AuthZ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AuthModule.Infrastructure.AuthZ;

internal static partial class ServiceCollectionExtension
{
    internal static IServiceCollection AddAuthZ(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasPermissionHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
        services.AddAuthorization();
        return services;
    }
}