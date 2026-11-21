using CartModule.Application.Repositories;
using CartModule.Infrastructure.Persistence;
using CartModule.Infrastructure.Repositories;
using CommonModule.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CartModule.Presentation.ModuleRegistration;

public static class ModuleRegistration
{
    public static IServiceCollection RegisterModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCartPersistence(configuration);
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddControllers().AddApplicationPart(typeof(ModuleRegistration).Assembly);
        services.AddMediatR(options => options.RegisterServicesFromAssembly(typeof(ModuleRegistration).Assembly));
        services.AddValidatorsFromAssembly(typeof(ModuleRegistration).Assembly, includeInternalTypes: true);
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));
        return services;
    }
}
