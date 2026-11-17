using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PaymentRefundModule.Infrastructure.Persistence;
public static class PersistenceRegistration
{
    public static IServiceCollection AddPaymentRefundPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PaymentRefundDbContext>(options => ConfigureOptions(options, configuration));
        return services;
    }

    internal static void ConfigureOptions(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        var connectionString = configuration["PAYMENTREFUND:DbConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Configure PAYMENTREFUND__DbConnectionString for PaymentRefundModule persistence.");
        }

        options.UseNpgsql(connectionString, postgres =>
        {
            postgres.MigrationsAssembly(typeof(PaymentRefundDbContext).Assembly.GetName().Name);
            postgres.MigrationsHistoryTable("__EFMigrationsHistory", PaymentRefundDbContext.Schema);
        });
    }
}
