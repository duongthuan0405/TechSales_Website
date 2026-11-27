using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CatalogModule.Application.Repositories;
using CatalogModule.Application.Services;
using CatalogModule.Application.Ports;
using CommonModule.Presentation.Configuration;
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
        services.AddScoped<CatalogModule.Contracts.IProductCatalog, ProductCatalog>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IExecuteAtomically, ExecuteAtomically>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductUnitOfWork, ProductUnitOfWork>();
        services.AddScoped<IProductImageStorage, LocalProductImageStorage>();
        services.AddScoped<IProductReferenceData, ProductReferenceData>();
        services.AddVietnameseResponses();
        services.AddAuthorization();
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
