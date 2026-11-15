using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<PermissionDbModel>
{
    public void Configure(EntityTypeBuilder<PermissionDbModel> builder)
    {
        builder.ToTable("Permission");
        builder.HasKey(x => x.id);
        builder.Property(x => x.code).IsRequired().HasMaxLength(100);
        builder.Property(x => x.name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.module).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => x.code).IsUnique();
        builder.Property(x => x.created_at).HasDefaultValueSql("now()");
    }
}
