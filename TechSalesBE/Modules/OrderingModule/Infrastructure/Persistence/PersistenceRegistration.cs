using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OrderingModule.Infrastructure.Persistence;
public static class PersistenceRegistration
{
    public static IServiceCollection AddOrderingPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderingDbContext>(options => ConfigureOptions(options, configuration));
        return services;
    }

    internal static void ConfigureOptions(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        var connectionString = configuration["ORDERING:DbConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Configure ORDERING__DbConnectionString for OrderingModule persistence.");
        }

        options.UseNpgsql(connectionString, postgres =>
        {
            postgres.MigrationsAssembly(typeof(OrderingDbContext).Assembly.GetName().Name);
            postgres.MigrationsHistoryTable("__EFMigrationsHistory", OrderingDbContext.Schema);
        });
    }
}
