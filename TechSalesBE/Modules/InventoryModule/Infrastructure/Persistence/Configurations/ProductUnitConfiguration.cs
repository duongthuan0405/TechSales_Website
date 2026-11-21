using InventoryModule.Domain.Constants;
using InventoryModule.Domain.Enums;
using InventoryModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryModule.Infrastructure.Persistence.Configurations;

internal sealed class ProductUnitConfiguration : IEntityTypeConfiguration<ProductUnitDbModel>
{
    public void Configure(EntityTypeBuilder<ProductUnitDbModel> builder)
    {
        builder.ToTable("ProductUnit");
        builder.HasKey(e => new { e.ProductId, e.SerialNumber });
        builder.Property(e => e.ProductId).HasColumnName("ProductId").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
        builder.Property(e => e.SerialNumber)
            .HasColumnName("SerialNumber")
            .HasColumnType($"character varying({ProductUnitConstants.MaxSerialNumberLength})")
            .HasMaxLength(ProductUnitConstants.MaxSerialNumberLength)
            .IsRequired()
            .ValueGeneratedNever();
        builder.Property(e => e.Status).HasColumnName("Status").HasColumnType("character varying(50)").HasConversion<string>().HasMaxLength(50).IsRequired();
    }
}
