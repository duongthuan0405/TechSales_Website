namespace AuthModule.Infrastructure.Persistence.Models;

internal sealed class RoleDbModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<RolePermissionDbModel> RolePermissions { get; set; } = new HashSet<RolePermissionDbModel>();
    public ICollection<UserRoleDbModel> UserRoles { get; set; } = new HashSet<UserRoleDbModel>();
}
