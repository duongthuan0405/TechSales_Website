using ShippingModule.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ShippingModule.Presentation.ModuleRegistration;

public static class ModuleRegistration
{

    public static IServiceCollection RegisterModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddShippingPersistence(configuration);
        services.AddControllers().AddApplicationPart(typeof(ModuleRegistration).Assembly);
        return services;
    }
}