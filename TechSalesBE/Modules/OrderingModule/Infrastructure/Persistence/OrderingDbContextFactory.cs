using CommonModule.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderingModule.Infrastructure.Persistence;

public sealed class OrderingDbContextFactory : IDesignTimeDbContextFactory<OrderingDbContext>
{
    public OrderingDbContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfiguration.Create(args);
        var options = new DbContextOptionsBuilder<OrderingDbContext>();
        PersistenceRegistration.ConfigureOptions(options, configuration);
        return new OrderingDbContext(options.Options);
    }
}