using AuthModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthModule.Infrastructure.Persistence;

public sealed class AuthDbContext : DbContext
{
    internal DbSet<UserDbModel> Users => Set<UserDbModel>();
    internal DbSet<UserOtpDbModel> UserOtps => Set<UserOtpDbModel>();
    internal DbSet<UserProfileDbModel> UserProfiles => Set<UserProfileDbModel>();
    internal DbSet<RoleDbModel> Roles => Set<RoleDbModel>();
    internal DbSet<PermissionDbModel> Permissions => Set<PermissionDbModel>();
    internal DbSet<RolePermissionDbModel> RolePermissions => Set<RolePermissionDbModel>();
    internal DbSet<UserRoleDbModel> UserRoles => Set<UserRoleDbModel>();
    internal DbSet<RefreshSessionDbModel> RefreshSessions => Set<RefreshSessionDbModel>();

    public const string Schema = "identify_access";

    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AuthDbContext).Assembly,
            type => type.Namespace?.StartsWith("AuthModule", StringComparison.Ordinal) ?? false);
    }
}
