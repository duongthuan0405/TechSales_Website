using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItemDbModel>
{
    public void Configure(EntityTypeBuilder<OrderItemDbModel> builder)
    {
        builder.ToTable("OrderItem");
        
        builder.HasKey(x => new { x.order_id, x.product_id });
        
        builder.Property(x => x.price).HasPrecision(12, 2);

        builder.HasOne(x => x.order)
            .WithMany(o => o.order_items)
            .HasForeignKey(x => x.order_id);

        builder.HasOne(x => x.product)
            .WithMany(p => p.order_items) // need product to have items
            .HasForeignKey(x => x.product_id);
    }
}
