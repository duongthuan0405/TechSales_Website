using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Persistence.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermissionDbModel>
{
    public void Configure(EntityTypeBuilder<RolePermissionDbModel> builder)
    {
        builder.ToTable("RolePermission");
        builder.HasKey(x => new { x.role_id, x.permission_id });

        builder.HasOne(x => x.role)
            .WithMany(r => r.role_permissions)
            .HasForeignKey(x => x.role_id);

        builder.HasOne(x => x.permission)
            .WithMany(p => p.role_permissions)
            .HasForeignKey(x => x.permission_id);
    }
}
