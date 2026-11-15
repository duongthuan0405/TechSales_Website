using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TechSalesManagement.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        var connectionString = Environment.GetEnvironmentVariable("DB__ConnectionString");

        services.AddDbContext<TechSalesDbContext>(options =>
            options.UseNpgsql(connectionString,
                b => b.MigrationsAssembly(typeof(TechSalesDbContext).Assembly.FullName)));

        return services;
    }
}
