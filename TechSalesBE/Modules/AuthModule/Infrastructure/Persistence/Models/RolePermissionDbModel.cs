namespace AuthModule.Infrastructure.Persistence.Models;

internal sealed class RolePermissionDbModel
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    public RoleDbModel Role { get; set; } = null!;
    public PermissionDbModel Permission { get; set; } = null!;
}
