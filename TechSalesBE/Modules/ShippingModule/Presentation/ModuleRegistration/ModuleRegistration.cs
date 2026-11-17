using CommonModule.Presentation.Configuration;
using ShippingModule.Application.Behaviors;
using ShippingModule.Presentation.Authorization;
using CommonModule.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShippingModule.Application.Services;
using ShippingModule.Infrastructure.Services;
using ShippingModule.Application.Repositories;
using ShippingModule.Infrastructure.Persistence;
using ShippingModule.Infrastructure.Repositories;

namespace ShippingModule.Presentation.ModuleRegistration;

public static class ModuleRegistration
{
    public static IServiceCollection RegisterModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddVietnameseResponses();
        services.AddShippingPersistence(configuration);
        services.AddAuthorization(options =>
        {
            options.AddPolicy(ShippingAuthorization.ManageFeesPolicy, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(ShippingAuthorization.BusinessAdminRole, ShippingAuthorization.TechnicalAdminRole);
            });
        });
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IShippingFeeRepository, ShippingFeeRepository>();
        services.AddScoped<IShippingAddressRepository, ShippingAddressRepository>();
        services.AddScoped<IExecuteAtomically, ExecuteAtomically>();
        services.AddControllers().AddApplicationPart(typeof(ModuleRegistration).Assembly);
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(ModuleRegistration).Assembly);
        });
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(BusinessRuleExceptionBehavior<,>)));
        services.AddValidatorsFromAssembly(typeof(ModuleRegistration).Assembly, includeInternalTypes: true);
        return services;
    }
}
