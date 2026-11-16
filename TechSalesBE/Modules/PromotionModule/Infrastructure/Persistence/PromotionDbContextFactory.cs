using CommonModule.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PromotionModule.Infrastructure.Persistence;

public sealed class PromotionDbContextFactory : IDesignTimeDbContextFactory<PromotionDbContext>
{
    public PromotionDbContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfiguration.Create(args);
        var options = new DbContextOptionsBuilder<PromotionDbContext>();
        PersistenceRegistration.ConfigureOptions(options, configuration);
        return new PromotionDbContext(options.Options);
    }
}