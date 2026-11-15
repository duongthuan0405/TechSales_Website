using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<NotificationDbModel>
{
    public void Configure(EntityTypeBuilder<NotificationDbModel> builder)
    {
        builder.ToTable("Notification");
        
        builder.HasKey(x => x.id);
        
        builder.Property(x => x.is_read).HasDefaultValue(false);
        
        builder.Property(x => x.title).HasMaxLength(255);
        
        builder.HasIndex(x => x.user_id);
        builder.Property(x => x.created_at).HasDefaultValueSql("now()");

        builder.HasOne(x => x.user)
            .WithMany(u => u.notifications)
            .HasForeignKey(x => x.user_id);
    }
}
