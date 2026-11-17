using CommonModule.Application.Behaviors;
using CommonModule.Presentation.Configuration;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PromotionModule.Application.Repositories;
using PromotionModule.Application.Services;
using PromotionModule.Infrastructure.Persistence;
using PromotionModule.Infrastructure.Repositories;
using PromotionModule.Infrastructure.Services;

namespace PromotionModule.Presentation.ModuleRegistration;

public static class ModuleRegistration
{
    public static IServiceCollection RegisterModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddVietnameseResponses();
        services.AddPromotionPersistence(configuration);
        services.AddScoped<IVoucherRepository, VoucherRepository>();
        services.AddScoped<IExecuteAtomically, ExecuteAtomically>();
        services.AddControllers().AddApplicationPart(typeof(ModuleRegistration).Assembly);
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(ModuleRegistration).Assembly);
        });
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));
        services.AddValidatorsFromAssembly(typeof(ModuleRegistration).Assembly, includeInternalTypes: true);
        services.AddAuthorization();
        return services;
    }
}
