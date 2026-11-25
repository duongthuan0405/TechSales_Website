using System;
using System.Collections.Generic;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class PermissionDbModel
{
    public Guid id { get; set; }
    public string code { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string module { get; set; } = string.Empty;
    public DateTimeOffset created_at { get; set; }
    public DateTimeOffset? updated_at { get; set; }

    // Navigation collections
    public ICollection<RolePermissionDbModel> role_permissions { get; set; } = new HashSet<RolePermissionDbModel>();
}
