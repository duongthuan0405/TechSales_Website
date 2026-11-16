using ShippingModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShippingModule.Infrastructure.Persistence.Configurations;

internal sealed class ProviceConfiguration : IEntityTypeConfiguration<ProviceDbModel>
{
    public void Configure(EntityTypeBuilder<ProviceDbModel> builder)
    {
        builder.ToTable("Provice");
        builder.HasKey(e => e.Code);
        builder.Property(e => e.Code).HasColumnName("Code").HasColumnType("character(7)").HasMaxLength(7).IsRequired().ValueGeneratedNever();
        builder.Property(e => e.Name).HasColumnName("Name").HasColumnType("character varying(100)").HasMaxLength(100).IsRequired(false);
    }
}
