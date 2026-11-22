using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ProductModule.Infrastructure.Persistence;
public static class PersistenceRegistration
{
    public static IServiceCollection AddProductPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProductDbContext>(options => ConfigureOptions(options, configuration));
        return services;
    }

    internal static void ConfigureOptions(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        // Keep the existing development configuration working during the module rename.
        string? connectionString = configuration["PRODUCT:DbConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = configuration["CATALOG:DbConnectionString"];
        }
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Cấu hình PRODUCT__DbConnectionString cho ProductModule (hỗ trợ key CATALOG__DbConnectionString cũ).");
        }

        options.UseNpgsql(connectionString, postgres =>
        {
            postgres.MigrationsAssembly(typeof(ProductDbContext).Assembly.GetName().Name);
            postgres.MigrationsHistoryTable("__EFMigrationsHistory", ProductDbContext.Schema);
        });
    }
}
