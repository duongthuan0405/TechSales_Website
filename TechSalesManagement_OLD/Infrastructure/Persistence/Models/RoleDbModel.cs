using System;
using System.Collections.Generic;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class RoleDbModel
{
    public Guid id { get; set; }
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public DateTimeOffset created_at { get; set; }

    // Navigation collections
    public ICollection<UserRoleDbModel> user_roles { get; set; } = new HashSet<UserRoleDbModel>();
    public ICollection<RolePermissionDbModel> role_permissions { get; set; } = new HashSet<RolePermissionDbModel>();
}
