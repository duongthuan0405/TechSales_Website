using AuthModule.Application.Ports;

namespace TechSalesBE.WebAPI.Integration;

internal static class IntegrationRegistration
{
    internal static IServiceCollection RegisterModule(IServiceCollection services)
    {
        services.AddScoped<IAuthEmailAdapter, AuthEmailAdapter>();
        return services;
    }
}
