using OrderingModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderingModule.Infrastructure.Persistence.Configurations;

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItemDbModel>
{
    public void Configure(EntityTypeBuilder<OrderItemDbModel> builder)
    {
        builder.ToTable("OrderItem");
        builder.HasKey(e => new { e.OrderId, e.ProductId });
        builder.Property(e => e.OrderId).HasColumnName("OrderId").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
        builder.Property(e => e.ProductId).HasColumnName("ProductId").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
        builder.Property(e => e.Price).HasColumnName("Price").HasColumnType("bigint").IsRequired();
        builder.Property(e => e.Quantity).HasColumnName("Quantity").HasColumnType("integer").IsRequired();
        builder.HasOne(e => e.Order).WithMany(e => e.Items).HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Restrict);
    }
}
