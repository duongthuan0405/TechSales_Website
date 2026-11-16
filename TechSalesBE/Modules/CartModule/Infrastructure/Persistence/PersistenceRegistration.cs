using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CartModule.Infrastructure.Persistence;

public static class PersistenceRegistration
{
    public static IServiceCollection AddCartPersistence(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CartDbContext>(options => ConfigureOptions(options, configuration));

        return services;
    }

    internal static void ConfigureOptions(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        var connectionString = configuration["CART:DbConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Configure CART__DbConnectionString for CartModule persistence.");

        options.UseNpgsql(connectionString, postgres =>
        {
            postgres.MigrationsAssembly(typeof(CartDbContext).Assembly.GetName().Name);
            postgres.MigrationsHistoryTable("__EFMigrationsHistory", CartDbContext.Schema);
        });
    }
}