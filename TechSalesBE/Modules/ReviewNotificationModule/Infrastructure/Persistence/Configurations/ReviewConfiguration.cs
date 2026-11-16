using ReviewNotificationModule.Domain.Enums;
using ReviewNotificationModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ReviewNotificationModule.Infrastructure.Persistence.Configurations;

internal sealed class ReviewConfiguration : IEntityTypeConfiguration<ReviewDbModel>
{
    public void Configure(EntityTypeBuilder<ReviewDbModel> builder)
    {
        builder.ToTable("Review");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.UserId).HasColumnName("UserId").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.ProductId).HasColumnName("ProductId").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.Rating).HasColumnName("Rating").HasColumnType("integer").IsRequired();
        builder.Property(e => e.Comment).HasColumnName("Comment").HasColumnType("text").IsRequired(false);
        builder.Property(e => e.Status).HasColumnName("Status").HasColumnType("character varying(50)").HasConversion<string>().HasMaxLength(50).IsRequired(false);
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("timestamp with time zone").IsRequired(false);
        builder.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasColumnType("timestamp with time zone").IsRequired(false);
    }
}
