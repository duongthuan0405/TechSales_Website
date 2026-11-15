using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<ProductDbModel>
{
    public void Configure(EntityTypeBuilder<ProductDbModel> builder)
    {
        builder.ToTable("Product");

        builder.HasKey(x => x.id);

        builder.Property(x => x.name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.brand)
            .HasMaxLength(255);

        builder.Property(x => x.created_at)
            .HasDefaultValueSql("now()");

        builder.HasOne(x => x.category)
            .WithMany(c => c.products)
            .HasForeignKey(x => x.category_id);
    }
}
