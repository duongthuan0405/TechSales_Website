namespace AuthModule.Infrastructure.Persistence.Models;

internal sealed class PermissionDbModel
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<RolePermissionDbModel> RolePermissions { get; set; } = new HashSet<RolePermissionDbModel>();
}
