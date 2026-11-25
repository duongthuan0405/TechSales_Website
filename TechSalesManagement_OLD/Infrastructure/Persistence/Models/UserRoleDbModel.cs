using System;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class UserRoleDbModel
{
    public Guid user_id { get; set; }
    public Guid role_id { get; set; }

    // Navigation properties
    public UserDbModel user { get; set; } = null!;
    public RoleDbModel role { get; set; } = null!;
}
