using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ShippingModule.Infrastructure.Persistence;

public static class PersistenceRegistration
{
    public static IServiceCollection AddShippingPersistence(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ShippingDbContext>(options => ConfigureOptions(options, configuration));

        return services;
    }

    internal static void ConfigureOptions(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        var connectionString = configuration["SHIPPING:DbConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Configure SHIPPING__DbConnectionString for ShippingModule persistence.");

        options.UseNpgsql(connectionString, postgres =>
        {
            postgres.MigrationsAssembly(typeof(ShippingDbContext).Assembly.GetName().Name);
            postgres.MigrationsHistoryTable("__EFMigrationsHistory", ShippingDbContext.Schema);
        });
    }
}