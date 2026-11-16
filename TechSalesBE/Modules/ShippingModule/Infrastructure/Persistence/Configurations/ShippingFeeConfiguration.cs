using ShippingModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShippingModule.Infrastructure.Persistence.Configurations;

internal sealed class ShippingFeeConfiguration : IEntityTypeConfiguration<ShippingFeeDbModel>
{
    public void Configure(EntityTypeBuilder<ShippingFeeDbModel> builder)
    {
        builder.ToTable("ShippingFee");
        builder.HasKey(e => e.ProviceCode);
        builder.Property(e => e.ProviceCode).HasColumnName("ProviceCode").HasColumnType("character(7)").HasMaxLength(7).IsRequired().ValueGeneratedNever();
        builder.Property(e => e.Fee).HasColumnName("Fee").HasColumnType("bigint").IsRequired().HasDefaultValue(0L);
    }
}
