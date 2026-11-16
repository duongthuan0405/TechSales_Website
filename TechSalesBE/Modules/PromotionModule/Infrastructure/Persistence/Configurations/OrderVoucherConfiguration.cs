using PromotionModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PromotionModule.Infrastructure.Persistence.Configurations;

internal sealed class OrderVoucherConfiguration : IEntityTypeConfiguration<OrderVoucherDbModel>
{
    public void Configure(EntityTypeBuilder<OrderVoucherDbModel> builder)
    {
        builder.ToTable("OrderVoucher");
        builder.HasKey(e => new { e.OrderId, e.VoucherId });
        builder.Property(e => e.OrderId).HasColumnName("OrderId").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
        builder.Property(e => e.VoucherId).HasColumnName("VoucherId").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
    }
}
