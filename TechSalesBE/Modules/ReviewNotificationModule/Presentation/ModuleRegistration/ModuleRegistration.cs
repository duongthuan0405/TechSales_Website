using ReviewNotificationModule.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ReviewNotificationModule.Presentation.ModuleRegistration;

public static class ModuleRegistration
{

    public static IServiceCollection RegisterModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReviewNotificationPersistence(configuration);
        services.AddScoped<ReviewNotificationModule.Contracts.IProductRatings,
            ReviewNotificationModule.Infrastructure.ProductRatings>();
        services.AddControllers().AddApplicationPart(typeof(ModuleRegistration).Assembly);
        return services;
    }
}
