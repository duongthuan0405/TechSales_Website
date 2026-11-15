using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<InventoryDbModel>
{
    public void Configure(EntityTypeBuilder<InventoryDbModel> builder)
    {
        builder.ToTable("Inventory");
        
        builder.HasKey(x => x.product_id);
        
        builder.Property(x => x.quantity).IsRequired();
        builder.Property(x => x.reserved_quantity).HasDefaultValue(0);

        builder.HasOne(x => x.product)
            .WithOne(p => p.inventory)
            .HasForeignKey<InventoryDbModel>(x => x.product_id);
    }
}
