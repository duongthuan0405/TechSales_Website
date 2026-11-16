using PromotionModule.Domain.Enums;
using PromotionModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PromotionModule.Infrastructure.Persistence.Configurations;

internal sealed class VoucherConfiguration : IEntityTypeConfiguration<VoucherDbModel>
{
    public void Configure(EntityTypeBuilder<VoucherDbModel> builder)
    {
        builder.ToTable("Voucher");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.Code).HasColumnName("Code").HasColumnType("character varying(50)").HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.Code).IsUnique();
        builder.Property(e => e.Type).HasColumnName("Type").HasColumnType("character varying(50)").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.Value).HasColumnName("Value").HasColumnType("bigint").IsRequired(false);
        builder.Property(e => e.MaxUsage).HasColumnName("MaxUsage").HasColumnType("integer").IsRequired(false);
        builder.Property(e => e.UsedCount).HasColumnName("UsedCount").HasColumnType("integer").IsRequired(false).HasDefaultValue(0);
        builder.Property(e => e.MinOrderAmount).HasColumnName("MinOrderAmount").HasColumnType("bigint").IsRequired(false);
        builder.Property(e => e.StartDate).HasColumnName("StartDate").HasColumnType("timestamp with time zone").IsRequired(false);
        builder.Property(e => e.EndDate).HasColumnName("EndDate").HasColumnType("timestamp with time zone").IsRequired(false);
        builder.Property(e => e.IsActive).HasColumnName("IsActive").HasColumnType("boolean").IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("timestamp with time zone").IsRequired(false);
        builder.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasColumnType("timestamp with time zone").IsRequired(false).HasDefaultValueSql("now()");
    }
}
