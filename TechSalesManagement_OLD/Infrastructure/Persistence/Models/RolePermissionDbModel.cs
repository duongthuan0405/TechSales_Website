using System;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class RolePermissionDbModel
{
    public Guid role_id { get; set; }
    public Guid permission_id { get; set; }

    // Navigation properties
    public RoleDbModel role { get; set; } = null!;
    public PermissionDbModel permission { get; set; } = null!;
}
