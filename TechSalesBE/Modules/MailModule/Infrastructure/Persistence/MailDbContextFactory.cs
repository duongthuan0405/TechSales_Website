using CommonModule.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MailModule.Infrastructure.Persistence;

public sealed class MailDbContextFactory : IDesignTimeDbContextFactory<MailDbContext>
{
    public MailDbContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfiguration.Create(args);
        var options = new DbContextOptionsBuilder<MailDbContext>();
        PersistenceRegistration.ConfigureOptions(options, configuration);
        return new MailDbContext(options.Options);
    }
}