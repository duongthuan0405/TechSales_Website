using ReviewNotificationModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ReviewNotificationModule.Infrastructure.Persistence.Configurations;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<NotificationDbModel>
{
    public void Configure(EntityTypeBuilder<NotificationDbModel> builder)
    {
        builder.ToTable("Notification");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.UserId).HasColumnName("UserId").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.Title).HasColumnName("Title").HasColumnType("character varying(255)").HasMaxLength(255).IsRequired(false);
        builder.Property(e => e.Content).HasColumnName("Content").HasColumnType("text").IsRequired(false);
        builder.Property(e => e.IsRead).HasColumnName("IsRead").HasColumnType("boolean").IsRequired(false).HasDefaultValue(false);
        builder.Property(e => e.RefTo).HasColumnName("RefTo").HasColumnType("uuid").IsRequired(false);
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("timestamp with time zone").IsRequired(false);
    }
}
