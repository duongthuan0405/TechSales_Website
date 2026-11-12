namespace AuthModule.Domain.Entities;

internal class RolePermission
{
    private Guid _roleId = Guid.Empty;
    private Guid _permissionRole = Guid.Empty;

    internal RolePermission(Guid roleId, Guid permissionRole)
    {
        _roleId = roleId;
        _permissionRole = permissionRole;
    }

    internal RolePermission() {}

    internal Guid RoleId { get => _roleId; set => _roleId = value; }
    internal Guid PermissionRole { get => _permissionRole; set => _permissionRole = value; }
}