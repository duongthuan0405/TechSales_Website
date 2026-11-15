using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommonModule.Presentation.Middlewares;

namespace CommonModule.Presentation.ModuleRegistration;
public static class ServiceCollectionExtension
{
    public static IServiceCollection RegisterCommonModule(this IServiceCollection services, IConfiguration configuration) 
    {
        services.AddTransient<GlobalExceptionMiddleware>();
        return services;
    }
}
