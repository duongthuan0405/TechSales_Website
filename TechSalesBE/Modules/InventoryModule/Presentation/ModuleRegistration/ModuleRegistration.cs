using InventoryModule.Application.Repositories;
using InventoryModule.Infrastructure.Persistence;
using InventoryModule.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryModule.Presentation.ModuleRegistration;

public static class ModuleRegistration
{

    public static IServiceCollection RegisterModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInventoryPersistence(configuration);
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddControllers().AddApplicationPart(typeof(ModuleRegistration).Assembly);
        return services;
    }
}
