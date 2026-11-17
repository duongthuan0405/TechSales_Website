using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MailModule.Infrastructure.Persistence;
public static class PersistenceRegistration
{
    public static IServiceCollection AddMailPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MailDbContext>(options => ConfigureOptions(options, configuration));
        return services;
    }

    internal static void ConfigureOptions(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        var connectionString = configuration["MAIL:DbConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Configure MAIL__DbConnectionString for MailModule persistence.");
        }

        options.UseNpgsql(connectionString, postgres =>
        {
            postgres.MigrationsAssembly(typeof(MailDbContext).Assembly.GetName().Name);
            postgres.MigrationsHistoryTable("__EFMigrationsHistory", MailDbContext.Schema);
        });
    }
}
