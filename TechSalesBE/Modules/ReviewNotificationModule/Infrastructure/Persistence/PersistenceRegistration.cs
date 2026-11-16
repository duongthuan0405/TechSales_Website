using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ReviewNotificationModule.Infrastructure.Persistence;

public static class PersistenceRegistration
{
    public static IServiceCollection AddReviewNotificationPersistence(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ReviewNotificationDbContext>(options => ConfigureOptions(options, configuration));

        return services;
    }

    internal static void ConfigureOptions(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        var connectionString = configuration["REVIEWNOTIFICATION:DbConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Configure REVIEWNOTIFICATION__DbConnectionString for ReviewNotificationModule persistence.");

        options.UseNpgsql(connectionString, postgres =>
        {
            postgres.MigrationsAssembly(typeof(ReviewNotificationDbContext).Assembly.GetName().Name);
            postgres.MigrationsHistoryTable("__EFMigrationsHistory", ReviewNotificationDbContext.Schema);
        });
    }
}