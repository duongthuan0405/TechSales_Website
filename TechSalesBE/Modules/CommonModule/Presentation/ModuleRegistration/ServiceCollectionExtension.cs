using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommonModule.Presentation.ModuleRegistration;
public static class ServiceCollectionExtension
{
    public static IServiceCollection RegisterCommonModule(this IServiceCollection services, IConfiguration configuration) 
    {
        return services;
    }
}