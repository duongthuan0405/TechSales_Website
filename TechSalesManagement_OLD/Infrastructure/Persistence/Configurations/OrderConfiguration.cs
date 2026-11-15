using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<OrderDbModel>
{
    public void Configure(EntityTypeBuilder<OrderDbModel> builder)
    {
        builder.ToTable("Order");

        builder.HasKey(x => x.id);

        builder.Property(x => x.status)
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.total_product_amount).HasPrecision(12, 2);
        builder.Property(x => x.shipping_fee).HasPrecision(12, 2);
        builder.Property(x => x.discount_amount).HasPrecision(12, 2);
        builder.Property(x => x.total_amount).HasPrecision(12, 2).IsRequired();

        builder.Property(x => x.shipping_address_snapshot).IsRequired();

        builder.Property(x => x.created_at)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.HasOne(x => x.user)
            .WithMany(u => u.orders) // Wait, I haven't added orders collection to User entity yet! I must do that now.
            .HasForeignKey(x => x.user_id);
    }
}
