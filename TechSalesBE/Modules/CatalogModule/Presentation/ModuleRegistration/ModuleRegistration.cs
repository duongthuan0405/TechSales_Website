using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CatalogModule.Application.Repositories;
using CatalogModule.Application.Services;
using CatalogModule.Infrastructure.Persistence;
using CatalogModule.Infrastructure.Repositories;
using CatalogModule.Infrastructure.Services;
using CommonModule.Application.Behaviors;

namespace CatalogModule.Presentation.ModuleRegistration;

public static class ModuleRegistration
{
    public static IServiceCollection RegisterModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCatalogPersistence(configuration);
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IExecuteAtomically, ExecuteAtomically>();
        services.AddControllers().AddApplicationPart(typeof(ModuleRegistration).Assembly);
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(ModuleRegistration).Assembly);
        });
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));
        services.AddValidatorsFromAssembly(typeof(ModuleRegistration).Assembly, includeInternalTypes: true);
        return services;
    }
}
