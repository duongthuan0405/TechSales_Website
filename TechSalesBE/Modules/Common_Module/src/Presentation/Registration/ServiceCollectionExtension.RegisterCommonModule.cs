using CommonModule.Mediator.Command.Abstract;
using CommonModule.Mediator.Command.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommonModule.Extensions;
public static partial class ServiceCollectionExtension
{
    public static IServiceCollection RegisterCommonModule(this IServiceCollection services, IConfiguration configuration) 
    {
        services.AddCommandExecutor();
        services.AddEventPublisher();
        return services;
    }
}