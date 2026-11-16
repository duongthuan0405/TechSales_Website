using InventoryModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryModule.Infrastructure.Persistence.Configurations;

internal sealed class InvetoryConfiguration : IEntityTypeConfiguration<InvetoryDbModel>
{
    public void Configure(EntityTypeBuilder<InvetoryDbModel> builder)
    {
        builder.ToTable("Invetory");
        builder.HasKey(e => e.ProductId);
        builder.Property(e => e.ProductId).HasColumnName("ProductId").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
        builder.Property(e => e.Quantity).HasColumnName("Quantity").HasColumnType("integer").IsRequired();
        builder.Property(e => e.ReservedQuantity).HasColumnName("ReservedQuantity").HasColumnType("integer").IsRequired(false).HasDefaultValue(0);
    }
}
