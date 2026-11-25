using System;
using TechSalesManagement.Domain.Common;

namespace TechSalesManagement.Domain.Entities;

public class Role
{
    public Guid id { get; set; }
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? updatedAt { get; set; }

    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;

    public List<Permission> permissions { get; set; } = new();

    public Role(string name, string description)
    {
        this.name = name;
        this.description = description;
    }

    public Role() { }

    public void UpdatePermissions(List<Permission> newPermissions)
    {
        this.permissions = newPermissions ?? new();
        this.updatedAt = DateTimeOffset.UtcNow;
    }
}
