using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<CategoryDbModel>
{
    public void Configure(EntityTypeBuilder<CategoryDbModel> builder)
    {
        builder.ToTable("Category");
        builder.HasKey(x => x.id);
        builder.Property(x => x.name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.created_at).HasDefaultValueSql("now()");
    }
}
