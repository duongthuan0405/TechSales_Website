using ReviewNotificationModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ReviewNotificationModule.Infrastructure.Persistence.Configurations;

internal sealed class ReviewResponseConfiguration : IEntityTypeConfiguration<ReviewResponseDbModel>
{
    public void Configure(EntityTypeBuilder<ReviewResponseDbModel> builder)
    {
        builder.ToTable("ReviewResponse");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.ReviewId).HasColumnName("ReviewId").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.UserId).HasColumnName("UserId").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.Content).HasColumnName("Content").HasColumnType("text").IsRequired(false);
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("timestamp with time zone").IsRequired(false);
        builder.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasColumnType("timestamp with time zone").IsRequired(false);
        builder.HasOne(e => e.Review).WithMany(e => e.Responses).HasForeignKey(e => e.ReviewId).OnDelete(DeleteBehavior.Restrict);
    }
}
