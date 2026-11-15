using System;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class AuditLogDbModel
{
    public Guid id { get; set; }
    public Guid? user_id { get; set; }
    public string action { get; set; } = string.Empty;
    public string table_name { get; set; } = string.Empty;
    public string primary_key { get; set; } = string.Empty;
    public string? old_values { get; set; }
    public string? new_values { get; set; }
    public string? affected_columns { get; set; }
    public DateTimeOffset created_at { get; set; }

    // Navigation properties
    public UserDbModel? user { get; set; }
}
