using AuthModule.Application.Constants;
using AuthModule.Infrastructure.Persistence.Models;
namespace AuthModule.Infrastructure.Persistence;
internal static class AuthSeedData
{
    private static readonly Guid CustomerRoleId = Guid.Parse("75595ed2-8e03-476c-a59c-864fbc57b1a9");
    private static readonly Guid StaffRoleId = Guid.Parse("8e2a0a54-e882-4174-ae34-32f299096d13");
    private static readonly Guid BusinessAdminRoleId = Guid.Parse("c22cf7a1-67f6-479c-a3df-9504f8270fa6");
    private static readonly Guid TechnicalAdminRoleId = Guid.Parse("668a08c6-9b2d-4189-96f9-7cc07e5a3b5a");
    private static readonly Guid CustomersReadPermissionId = Guid.Parse("a0000000-0000-0000-0000-000000000001");
    private static readonly Guid StaffReadPermissionId = Guid.Parse("a0000000-0000-0000-0000-000000000002");
    private static readonly Guid UsersManagePermissionId = Guid.Parse("a0000000-0000-0000-0000-000000000003");
    private static readonly Guid CustomersLockPermissionId = Guid.Parse("a0000000-0000-0000-0000-000000000004");
    private static readonly Guid RbacManagePermissionId = Guid.Parse("a0000000-0000-0000-0000-000000000005");

    private static readonly DateTimeOffset CreatedAt = new(2026, 5, 12, 0, 0, 0, TimeSpan.Zero);
    internal static readonly RoleDbModel[] Roles =
    [
        new() { Id = CustomerRoleId, Name = UserRoleName.CUSTOMER, Description = "Customer account", CreatedAt = CreatedAt },
        new() { Id = StaffRoleId, Name = UserRoleName.STAFF, Description = "Sales and customer support staff", CreatedAt = CreatedAt },
        new() { Id = BusinessAdminRoleId, Name = UserRoleName.BUSINESS_ADMIN, Description = "Business administration", CreatedAt = CreatedAt },
        new() { Id = TechnicalAdminRoleId, Name = UserRoleName.TECHNICAL_ADMIN, Description = "Technical administration and RBAC", CreatedAt = CreatedAt }
    ];
    internal static readonly PermissionDbModel[] Permissions =
    [
        Permission(CustomersReadPermissionId, AuthPermission.CUSTOMERS_READ, "Read customers"),
        Permission(StaffReadPermissionId, AuthPermission.STAFF_READ, "Read staff"),
        Permission(UsersManagePermissionId, AuthPermission.USERS_MANAGE, "Create and update accounts"),
        Permission(CustomersLockPermissionId, AuthPermission.CUSTOMERS_LOCK, "Lock and unlock accounts within hierarchy"),
        Permission(RbacManagePermissionId, AuthPermission.RBAC_MANAGE, "Read and manage roles and permissions")
    ];
    // Customer has no administrative permissions.
    internal static readonly List<RolePermissionDbModel> RolePermissions = new List<RolePermissionDbModel>
    {
        // Staff
        new RolePermissionDbModel
        {
            RoleId = StaffRoleId,
            PermissionId = CustomersReadPermissionId
        },
        new RolePermissionDbModel
        {
            RoleId = StaffRoleId,
            PermissionId = CustomersLockPermissionId
        },

        // BusinessAdmin
        new RolePermissionDbModel
        {
            RoleId = BusinessAdminRoleId,
            PermissionId = CustomersReadPermissionId
        },
        new RolePermissionDbModel
        {
            RoleId = BusinessAdminRoleId,
            PermissionId = StaffReadPermissionId
        },
        new RolePermissionDbModel
        {
            RoleId = BusinessAdminRoleId,
            PermissionId = UsersManagePermissionId
        },
        new RolePermissionDbModel
        {
            RoleId = BusinessAdminRoleId,
            PermissionId = CustomersLockPermissionId
        },

        // TechnicalAdmin
        new RolePermissionDbModel
        {
            RoleId = TechnicalAdminRoleId,
            PermissionId = CustomersReadPermissionId
        },
        new RolePermissionDbModel
        {
            RoleId = TechnicalAdminRoleId,
            PermissionId = StaffReadPermissionId
        },
        new RolePermissionDbModel
        {
            RoleId = TechnicalAdminRoleId,
            PermissionId = UsersManagePermissionId
        },
        new RolePermissionDbModel
        {
            RoleId = TechnicalAdminRoleId,
            PermissionId = CustomersLockPermissionId
        },
        new RolePermissionDbModel
        {
            RoleId = TechnicalAdminRoleId,
            PermissionId = RbacManagePermissionId
        },

    };

    private static PermissionDbModel Permission(Guid id, string code, string name)
    {
        PermissionDbModel permission = new PermissionDbModel
        {
            Id = id,
            Code = code,
            Name = name,
            Module = "Auth",
            CreatedAt = CreatedAt
        };

        return permission;
    }
}
