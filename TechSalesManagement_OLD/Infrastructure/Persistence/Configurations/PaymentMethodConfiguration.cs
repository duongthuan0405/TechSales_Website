using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethodDbModel>
{
    public void Configure(EntityTypeBuilder<PaymentMethodDbModel> builder)
    {
        builder.ToTable("PaymentMethod");
        builder.HasKey(x => x.id);
        builder.Property(x => x.name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.type).HasConversion<string>().HasMaxLength(50);
        builder.HasIndex(x => x.name).IsUnique();
    }
}
