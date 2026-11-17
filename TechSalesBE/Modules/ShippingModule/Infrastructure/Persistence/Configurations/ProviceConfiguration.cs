using ShippingModule.Domain.Constants;
using ShippingModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShippingModule.Infrastructure.Persistence.Configurations;

internal sealed class ProvinceConfiguration : IEntityTypeConfiguration<ProvinceDbModel>
{
    public void Configure(EntityTypeBuilder<ProvinceDbModel> builder)
    {
        builder.ToTable("Province");
        builder.HasKey(e => e.Code);
        builder.Property(e => e.Code).HasColumnName("Code").HasColumnType($"character({ShippingLimits.AdministrativeCodeMaxLength})").HasMaxLength(ShippingLimits.AdministrativeCodeMaxLength).IsRequired().ValueGeneratedNever();
        builder.Property(e => e.Name).HasColumnName("Name").HasColumnType("character varying(100)").HasMaxLength(100).IsRequired(false);
    }
}
