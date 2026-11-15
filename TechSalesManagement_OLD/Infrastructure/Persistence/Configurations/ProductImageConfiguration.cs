using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImageDbModel>
{
    public void Configure(EntityTypeBuilder<ProductImageDbModel> builder)
    {
        builder.ToTable("ProductImage");
        builder.HasKey(x => x.id);
        
        builder.Property(x => x.image_url).IsRequired();
        builder.Property(x => x.is_primary).HasDefaultValue(false);
        
        builder.HasIndex(x => x.product_id);
        builder.Property(x => x.created_at).HasDefaultValueSql("now()");

        builder.HasOne(x => x.product)
            .WithMany(p => p.product_images)
            .HasForeignKey(x => x.product_id);
    }
}
