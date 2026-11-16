using AuthModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthModule.Infrastructure.Persistence.Configurations;

internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRoleDbModel>
{
    public void Configure(EntityTypeBuilder<UserRoleDbModel> builder)
    {
        builder.ToTable("UserRole");
        builder.HasKey(e => new { e.UserId, e.RoleId });
        builder.Property(e => e.UserId).HasColumnName("UserId").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
        builder.Property(e => e.RoleId).HasColumnName("RoleId").HasColumnType("uuid").IsRequired().ValueGeneratedNever();
        builder.HasOne(e => e.User).WithMany(e => e.UserRoles).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Role).WithMany(e => e.UserRoles).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Restrict);
    }
}
