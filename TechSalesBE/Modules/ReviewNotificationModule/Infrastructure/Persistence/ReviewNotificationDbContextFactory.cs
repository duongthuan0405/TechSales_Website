using CommonModule.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ReviewNotificationModule.Infrastructure.Persistence;

public sealed class ReviewNotificationDbContextFactory : IDesignTimeDbContextFactory<ReviewNotificationDbContext>
{
    public ReviewNotificationDbContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfiguration.Create(args);
        var options = new DbContextOptionsBuilder<ReviewNotificationDbContext>();
        PersistenceRegistration.ConfigureOptions(options, configuration);
        return new ReviewNotificationDbContext(options.Options);
    }
}