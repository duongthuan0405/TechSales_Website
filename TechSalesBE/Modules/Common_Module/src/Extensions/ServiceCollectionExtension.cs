using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommonModule.src.Extensions;
public static class ServiceCollectionExtension
{
    public static IServiceCollection RegisterCommonModule(this IServiceCollection services, IConfiguration configuration) 
    {
        return services;
    }
}