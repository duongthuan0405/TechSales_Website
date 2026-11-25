using System;
using TechSalesManagement.Domain.Common;

namespace TechSalesManagement.Domain.Entities;

public class Permission
{
    public Guid id { get; set; }
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? updatedAt { get; set; }

    public string code { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string module { get; set; } = string.Empty;

    public Permission(string code, string name, string module)
    {
        this.code = code;
        this.name = name;
        this.module = module;
    }

    public Permission() { }
}
