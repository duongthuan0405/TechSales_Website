using ShippingModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShippingModule.Infrastructure.Persistence.Configurations;

internal sealed class WardConfiguration : IEntityTypeConfiguration<WardDbModel>
{
    public void Configure(EntityTypeBuilder<WardDbModel> builder)
    {
        builder.ToTable("Ward");
        builder.HasKey(e => e.Code);
        builder.Property(e => e.Code).HasColumnName("Code").HasColumnType("character(7)").HasMaxLength(7).IsRequired().ValueGeneratedNever();
        builder.Property(e => e.Name).HasColumnName("Name").HasColumnType("character varying(100)").HasMaxLength(100).IsRequired(false);
        builder.Property(e => e.ProvinceCode).HasColumnName("ProvinceCode").HasColumnType("character(7)").HasMaxLength(7).IsRequired();
        builder.HasOne(e => e.Province).WithMany(e => e.Wards).HasForeignKey(e => e.ProvinceCode).OnDelete(DeleteBehavior.Restrict);
    }
}
