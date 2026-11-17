using Microsoft.EntityFrameworkCore;

namespace MailModule.Infrastructure.Persistence;

public sealed class MailDbContext : DbContext
{
    public const string Schema = "email_sending";

    public MailDbContext(DbContextOptions<MailDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MailDbContext).Assembly,
            type => type.Namespace == "MailModule.Infrastructure.Persistence.Configurations"
                || (type.Namespace?.StartsWith("MailModule.Infrastructure.Persistence.Configurations.", StringComparison.Ordinal) ?? false));
    }
}
