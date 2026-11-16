using CommonModule.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AfterSaleModule.Infrastructure.Persistence;

public sealed class AfterSaleDbContextFactory : IDesignTimeDbContextFactory<AfterSaleDbContext>
{
    public AfterSaleDbContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfiguration.Create(args);
        var options = new DbContextOptionsBuilder<AfterSaleDbContext>();
        PersistenceRegistration.ConfigureOptions(options, configuration);
        return new AfterSaleDbContext(options.Options);
    }
}