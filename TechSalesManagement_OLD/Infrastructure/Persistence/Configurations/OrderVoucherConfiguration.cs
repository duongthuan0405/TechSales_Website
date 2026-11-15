using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class OrderVoucherConfiguration : IEntityTypeConfiguration<OrderVoucherDbModel>
{
    public void Configure(EntityTypeBuilder<OrderVoucherDbModel> builder)
    {
        builder.ToTable("OrderVoucher");
        builder.HasKey(x => new { x.order_id, x.voucher_id });

        builder.HasOne(x => x.order)
            .WithMany(o => o.order_vouchers)
            .HasForeignKey(x => x.order_id);

        builder.HasOne(x => x.voucher)
            .WithMany(v => v.order_vouchers)
            .HasForeignKey(x => x.voucher_id);
    }
}
