using ProductModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProductModule.Infrastructure.Persistence.Configurations;

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImageDbModel>
{
    public void Configure(EntityTypeBuilder<ProductImageDbModel> builder)
    {
        builder.ToTable("ProductImage");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.ProductId).HasColumnName("ProductId").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.ImageUrl).HasColumnName("ImageUrl").HasColumnType("character varying(500)").HasMaxLength(500).IsRequired();
        builder.Property(e => e.IsPrimary).HasColumnName("IsPrimary").HasColumnType("boolean").IsRequired(false).HasDefaultValue(false);
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("timestamp with time zone").IsRequired().HasDefaultValueSql("now()");
        builder.HasOne(e => e.Product).WithMany(e => e.Images).HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
