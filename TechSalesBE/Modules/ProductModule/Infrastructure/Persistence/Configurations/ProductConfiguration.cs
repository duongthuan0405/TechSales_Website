using ProductModule.Domain.Enums;
using ProductModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProductModule.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<ProductDbModel>
{
    public void Configure(EntityTypeBuilder<ProductDbModel> builder)
    {
        builder.ToTable("Product");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.Name).HasColumnName("Name").HasColumnType("character varying(255)").HasMaxLength(255).IsRequired();
        builder.Property(e => e.Description).HasColumnName("Description").HasColumnType("text").IsRequired();
        builder.Property(e => e.Price).HasColumnName("Price").HasColumnType("bigint").IsRequired(false);
        builder.Property(e => e.Status).HasColumnName("Status").HasColumnType("character varying(50)").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.BrandId).HasColumnName("BrandId").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.CategoryId).HasColumnName("CategoryId").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasColumnType("timestamp with time zone").IsRequired().HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt").HasColumnType("timestamp with time zone").IsRequired(false);
        builder.HasOne(e => e.Brand).WithMany(e => e.Products).HasForeignKey(e => e.BrandId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Category).WithMany(e => e.Products).HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
