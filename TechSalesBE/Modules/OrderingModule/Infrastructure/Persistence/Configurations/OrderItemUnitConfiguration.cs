using OrderingModule.Domain.Enums;
using OrderingModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderingModule.Infrastructure.Persistence.Configurations;

internal sealed class OrderItemUnitConfiguration : IEntityTypeConfiguration<OrderItemUnitDbModel>
{
    public void Configure(EntityTypeBuilder<OrderItemUnitDbModel> builder)
    {
        builder.ToTable("OrderItemUnit");
        builder.HasKey(e => new { e.OrderId, e.ProductId, e.SerialNumber });
        builder.Property(e => e.OrderId).HasColumnName("OrderId").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
        builder.Property(e => e.ProductId).HasColumnName("ProductId").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
        builder.Property(e => e.SerialNumber).HasColumnName("SerialNumber").HasColumnType("character varying(100)").HasMaxLength(100).IsRequired().ValueGeneratedNever();
        builder.Property(e => e.Status).HasColumnName("Status").HasColumnType("character varying(50)").HasConversion<string>().HasMaxLength(50).IsRequired(false);
        builder.HasOne(e => e.OrderItem).WithMany(e => e.Units).HasForeignKey(e => new { e.OrderId, e.ProductId }).OnDelete(DeleteBehavior.Cascade);
    }
}
