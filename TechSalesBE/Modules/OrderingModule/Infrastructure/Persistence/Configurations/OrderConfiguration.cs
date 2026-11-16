using OrderingModule.Domain.Enums;
using OrderingModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderingModule.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<OrderDbModel>
{
    public void Configure(EntityTypeBuilder<OrderDbModel> builder)
    {
        builder.ToTable("Order");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.UserId).HasColumnName("UserId").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.Status).HasColumnName("Status").HasColumnType("character varying(50)").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.TotalProductAmount).HasColumnName("TotalProductAmount").HasColumnType("bigint").IsRequired().HasDefaultValue(0L);
        builder.Property(e => e.ShippingFee).HasColumnName("ShippingFee").HasColumnType("bigint").IsRequired().HasDefaultValue(0L);
        builder.Property(e => e.DiscountAmount).HasColumnName("DiscountAmount").HasColumnType("bigint").IsRequired().HasDefaultValue(0L);
        builder.Property(e => e.TotalAmount).HasColumnName("TotalAmount").HasColumnType("bigint").IsRequired().HasDefaultValue(0L);
        builder.Property(e => e.ShippingAddressSnapshot).HasColumnName("ShippingAddressSnapshot").HasColumnType("text").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("timestamp with time zone").IsRequired().HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasColumnType("timestamp with time zone").IsRequired(false);
    }
}
