using CartModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CartModule.Infrastructure.Persistence.Configurations;

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItemDbModel>
{
    public void Configure(EntityTypeBuilder<CartItemDbModel> builder)
    {
        builder.ToTable("CartItem");
        builder.HasKey(e => new { e.CartId, e.ProductId });
        builder.Property(e => e.CartId).HasColumnName("CartId").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
        builder.Property(e => e.ProductId).HasColumnName("ProductId").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
        builder.Property(e => e.Quantity).HasColumnName("Quantity").HasColumnType("integer").IsRequired().HasDefaultValue(1);
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("timestamp with time zone").IsRequired().HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasColumnType("timestamp with time zone").IsRequired().HasDefaultValueSql("now()");
        builder.HasOne(e => e.Cart).WithMany(e => e.Items).HasForeignKey(e => e.CartId).OnDelete(DeleteBehavior.Restrict);
    }
}
