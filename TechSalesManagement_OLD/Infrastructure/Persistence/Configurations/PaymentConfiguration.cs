using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<PaymentDbModel>
{
    public void Configure(EntityTypeBuilder<PaymentDbModel> builder)
    {
        builder.ToTable("Payment");
        builder.HasKey(x => x.id);
        builder.Property(x => x.amount).HasPrecision(12, 2);
        builder.Property(x => x.status).HasConversion<string>().HasMaxLength(50);
        builder.HasIndex(x => x.order_id);
        builder.Property(x => x.created_at).HasDefaultValueSql("now()");
        builder.Property(x => x.updated_at).HasDefaultValueSql("now()");

        builder.HasOne(x => x.order)
            .WithMany(o => o.payments)
            .HasForeignKey(x => x.order_id);

        builder.HasOne(x => x.payment_method)
            .WithMany(pm => pm.payments)
            .HasForeignKey(x => x.payment_method_id);
    }
}
