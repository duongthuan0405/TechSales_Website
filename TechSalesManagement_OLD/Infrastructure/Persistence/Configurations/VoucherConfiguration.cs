using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class VoucherConfiguration : IEntityTypeConfiguration<VoucherDbModel>
{
    public void Configure(EntityTypeBuilder<VoucherDbModel> builder)
    {
        builder.ToTable("Voucher");
        builder.HasKey(x => x.id);
        builder.Property(x => x.code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.type).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.value).HasPrecision(12, 2);
        builder.Property(x => x.min_order_amount).HasPrecision(12, 2);
        builder.HasIndex(x => x.code).IsUnique();
        builder.Property(x => x.created_at).HasDefaultValueSql("now()");
    }
}
