using AuthModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthModule.Infrastructure.Persistence.Configurations;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermissionDbModel>
{
    public void Configure(EntityTypeBuilder<RolePermissionDbModel> builder)
    {
        builder.ToTable("RolePermission");
        builder.HasKey(e => new { e.RoleId, e.PermissionId });
        builder.Property(e => e.RoleId).HasColumnName("RoleId").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
        builder.Property(e => e.PermissionId).HasColumnName("PermissionId").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
        builder.HasOne(e => e.Role).WithMany(e => e.RolePermissions).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Permission).WithMany(e => e.RolePermissions).HasForeignKey(e => e.PermissionId).OnDelete(DeleteBehavior.Restrict);
    }
}
