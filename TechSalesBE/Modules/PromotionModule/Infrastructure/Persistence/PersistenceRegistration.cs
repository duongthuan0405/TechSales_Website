using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PromotionModule.Infrastructure.Persistence;
public static class PersistenceRegistration
{
    public static IServiceCollection AddPromotionPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PromotionDbContext>(options => ConfigureOptions(options, configuration));
        return services;
    }

    internal static void ConfigureOptions(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        var connectionString = configuration["PROMOTION:DbConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Configure PROMOTION__DbConnectionString for PromotionModule persistence.");
        }

        options.UseNpgsql(connectionString, postgres =>
        {
            postgres.MigrationsAssembly(typeof(PromotionDbContext).Assembly.GetName().Name);
            postgres.MigrationsHistoryTable("__EFMigrationsHistory", PromotionDbContext.Schema);
        });
    }
}
