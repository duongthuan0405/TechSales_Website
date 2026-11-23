using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogModule.Infrastructure.Persistence;
public static class PersistenceRegistration
{
    public static IServiceCollection AddCatalogPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(options => ConfigureOptions(options, configuration));
        return services;
    }

    internal static void ConfigureOptions(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        string? connectionString = configuration["CATALOG:DbConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Cấu hình CATALOG__DbConnectionString cho CatalogModule.");
        }

        options.UseNpgsql(connectionString, postgres =>
        {
            postgres.MigrationsAssembly(typeof(CatalogDbContext).Assembly.GetName().Name);
            postgres.MigrationsHistoryTable("__EFMigrationsHistory", CatalogDbContext.Schema);
        });
    }
}
