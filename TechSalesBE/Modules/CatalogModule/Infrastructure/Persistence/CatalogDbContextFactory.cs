using CommonModule.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CatalogModule.Infrastructure.Persistence;

public sealed class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfiguration.Create(args);
        var options = new DbContextOptionsBuilder<CatalogDbContext>();
        PersistenceRegistration.ConfigureOptions(options, configuration);
        return new CatalogDbContext(options.Options);
    }
}