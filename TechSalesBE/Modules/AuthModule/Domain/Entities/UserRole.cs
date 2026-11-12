namespace AuthModule.Domain.Entities;

internal class UserRole
{
    private Guid _userId = Guid.Empty;
    private Guid _roleId = Guid.Empty;

    internal Guid UserId { get => _userId; set => _userId = value; }
    internal Guid RoleId { get => _roleId; set => _roleId = value; }

    internal UserRole() {}
    
}