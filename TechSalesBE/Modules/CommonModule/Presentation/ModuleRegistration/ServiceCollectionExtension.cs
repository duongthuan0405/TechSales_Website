using CommonModule.Presentation.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CommonModule.Presentation.Middlewares;

namespace CommonModule.Presentation.ModuleRegistration;
public static class ModuleRegistration
{

    public static IServiceCollection RegisterModule(this IServiceCollection services, IConfiguration configuration) 
    {
        services.AddVietnameseResponses();
        services.AddTransient<GlobalExceptionMiddleware>();
        services.AddControllers();
        return services;
    }
}
