using ProductModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProductModule.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<CategoryDbModel>
{
    public void Configure(EntityTypeBuilder<CategoryDbModel> builder)
    {
        builder.ToTable("Category");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("Id").HasColumnType("uuid").IsRequired();
        builder.Property(e => e.Name).HasColumnName("Name").HasColumnType("character varying(100)").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAat).HasColumnName("CreatedAat").HasColumnType("timestamp with time zone").IsRequired().HasDefaultValueSql("now()");
    }
}
