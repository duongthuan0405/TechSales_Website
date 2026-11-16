using CommonModule.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PaymentRefundModule.Infrastructure.Persistence;

public sealed class PaymentRefundDbContextFactory : IDesignTimeDbContextFactory<PaymentRefundDbContext>
{
    public PaymentRefundDbContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfiguration.Create(args);
        var options = new DbContextOptionsBuilder<PaymentRefundDbContext>();
        PersistenceRegistration.ConfigureOptions(options, configuration);
        return new PaymentRefundDbContext(options.Options);
    }
}