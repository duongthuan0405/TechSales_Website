using ShippingModule.Domain.Constants;
using ShippingModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShippingModule.Infrastructure.Persistence.Configurations;

internal sealed class ShippingAddressConfiguration : IEntityTypeConfiguration<ShippingAddressDbModel>
{
    public void Configure(EntityTypeBuilder<ShippingAddressDbModel> builder)
    {
        builder.ToTable("ShippingAddress");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.UserId).HasColumnName("UserId").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.WardCode).HasColumnName("WardCode").HasColumnType($"character({ShippingLimits.AdministrativeCodeMaxLength})").HasMaxLength(ShippingLimits.AdministrativeCodeMaxLength).IsRequired();
        builder.Property(e => e.Detail).HasColumnName("Detail").HasColumnType($"character varying({ShippingLimits.AddressDetailMaxLength})").HasMaxLength(ShippingLimits.AddressDetailMaxLength).IsRequired();
        builder.Property(e => e.IsDefault).HasColumnName("IsDefault").HasColumnType("boolean").IsRequired(false).HasDefaultValue(false);
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("timestamp with time zone").IsRequired().HasDefaultValueSql("now()");
        builder.Property(e => e.UpdateAt).HasColumnName("UpdateAt").HasColumnType("timestamp with time zone").IsRequired(false);
        builder.Property(e => e.DeletedAt).HasColumnName("DeletedAt").HasColumnType("timestamp with time zone").IsRequired(false);
        builder.HasOne(e => e.Ward).WithMany(e => e.ShippingAddresses).HasForeignKey(e => e.WardCode).OnDelete(DeleteBehavior.Restrict);
    }
}
