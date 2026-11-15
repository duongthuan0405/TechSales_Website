using System;
using TechSalesManagement.Domain.Common;

namespace TechSalesManagement.Domain.Entities;

public class AuditLog
{
    public Guid id { get; set; }
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? updatedAt { get; set; }

    public Guid? userId { get; set; }
    public string action { get; set; } = string.Empty;
    public string tableName { get; set; } = string.Empty;
    public string primaryKey { get; set; } = string.Empty;
    public string? oldValues { get; set; }
    public string? newValues { get; set; }
    public string? affectedColumns { get; set; }

    public AuditLog(Guid? userId, string action, string tableName, string primaryKey)
    {
        this.userId = userId;
        this.action = action;
        this.tableName = tableName;
        this.primaryKey = primaryKey;
    }

    public AuditLog() { }
}
