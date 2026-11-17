using AuthModule.Application.Constants;
using AuthModule.Infrastructure.Persistence.Models;
namespace AuthModule.Infrastructure.Persistence;
internal static class AuthSeedData
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 5, 12, 0, 0, 0, TimeSpan.Zero);
    internal static readonly RoleDbModel[] Roles =
    [
        new() { Id = Guid.Parse("75595ed2-8e03-476c-a59c-864fbc57b1a9"), Name = UserRoleName.CUSTOMER, Description = "Customer account", CreatedAt = CreatedAt },
        new() { Id = Guid.Parse("8e2a0a54-e882-4174-ae34-32f299096d13"), Name = UserRoleName.STAFF, Description = "Sales and customer support staff", CreatedAt = CreatedAt },
        new() { Id = Guid.Parse("c22cf7a1-67f6-479c-a3df-9504f8270fa6"), Name = UserRoleName.BUSINESS_ADMIN, Description = "Business administration", CreatedAt = CreatedAt },
        new() { Id = Guid.Parse("668a08c6-9b2d-4189-96f9-7cc07e5a3b5a"), Name = UserRoleName.TECHNICAL_ADMIN, Description = "Technical administration and RBAC", CreatedAt = CreatedAt }
    ];
    internal static readonly PermissionDbModel[] Permissions =
    [
        Permission(1, AuthPermission.CUSTOMERS_READ, "Read customers"),
        Permission(2, AuthPermission.STAFF_READ, "Read staff"),
        Permission(3, AuthPermission.USERS_MANAGE, "Create and update accounts"),
        Permission(4, AuthPermission.CUSTOMERS_LOCK, "Lock and unlock accounts within hierarchy"),
        Permission(5, AuthPermission.RBAC_MANAGE, "Read and manage roles and permissions")
    ];
    internal static readonly RolePermissionDbModel[] RolePermissions = Roles.SelectMany(role => Permissions
        .Where(permission => role.Name == UserRoleName.TECHNICAL_ADMIN
            || (role.Name == UserRoleName.BUSINESS_ADMIN && permission.Code != AuthPermission.RBAC_MANAGE)
            || (role.Name == UserRoleName.STAFF && permission.Code is AuthPermission.CUSTOMERS_READ or AuthPermission.CUSTOMERS_LOCK))
        .Select(permission => new RolePermissionDbModel { RoleId = role.Id, PermissionId = permission.Id })).ToArray();
    private static PermissionDbModel Permission(int id, string code, string name) => new()
    {
        Id = Guid.Parse($"a0000000-0000-0000-0000-{id:D12}"), Code = code, Name = name,
        Module = "Auth", CreatedAt = CreatedAt
    };
}
