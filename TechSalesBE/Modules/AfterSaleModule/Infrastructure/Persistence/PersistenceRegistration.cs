using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AfterSaleModule.Infrastructure.Persistence;

public static class PersistenceRegistration
{
    public static IServiceCollection AddAfterSalePersistence(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AfterSaleDbContext>(options => ConfigureOptions(options, configuration));

        return services;
    }

    internal static void ConfigureOptions(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        var connectionString = configuration["AFTERSALE:DbConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Configure AFTERSALE__DbConnectionString for AfterSaleModule persistence.");

        options.UseNpgsql(connectionString, postgres =>
        {
            postgres.MigrationsAssembly(typeof(AfterSaleDbContext).Assembly.GetName().Name);
            postgres.MigrationsHistoryTable("__EFMigrationsHistory", AfterSaleDbContext.Schema);
        });
    }
}