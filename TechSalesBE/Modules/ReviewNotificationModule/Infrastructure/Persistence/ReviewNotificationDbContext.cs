using ReviewNotificationModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace ReviewNotificationModule.Infrastructure.Persistence;

public sealed class ReviewNotificationDbContext : DbContext
{
    internal DbSet<ReviewDbModel> Reviews => Set<ReviewDbModel>();
    internal DbSet<ReviewResponseDbModel> ReviewResponses => Set<ReviewResponseDbModel>();
    internal DbSet<NotificationDbModel> Notifications => Set<NotificationDbModel>();

    public const string Schema = "review_notification";

    public ReviewNotificationDbContext(DbContextOptions<ReviewNotificationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ReviewNotificationDbContext).Assembly,
            type => type.Namespace == "ReviewNotificationModule.Infrastructure.Persistence.Configurations"
                || (type.Namespace?.StartsWith("ReviewNotificationModule.Infrastructure.Persistence.Configurations.", StringComparison.Ordinal) ?? false));
    }
}