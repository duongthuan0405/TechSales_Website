using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class ShippingAddressConfiguration : IEntityTypeConfiguration<ShippingAddressDbModel>
{
    public void Configure(EntityTypeBuilder<ShippingAddressDbModel> builder)
    {
        builder.ToTable("ShippingAddress");
        builder.HasKey(x => x.id);
        builder.Property(x => x.province).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ward).IsRequired().HasMaxLength(100);
        builder.Property(x => x.detail).IsRequired().HasMaxLength(500);
        builder.HasIndex(x => x.user_id);
        builder.Property(x => x.created_at).HasDefaultValueSql("now()");

        builder.HasOne(x => x.user)
            .WithMany(u => u.shipping_addresses)
            .HasForeignKey(x => x.user_id);
    }
}
