using System;

namespace TechSalesManagement.Domain.Entities;

public class SystemSetting
{
    public string key { get; set; } = string.Empty; // PK
    public string value { get; set; } = string.Empty;
    public string? description { get; set; }
    public DateTimeOffset updatedAt { get; set; } = DateTimeOffset.UtcNow;

    public SystemSetting(string key, string value, string? description = null)
    {
        this.key = key;
        this.value = value;
        this.description = description;
    }

    public SystemSetting() { }
}
