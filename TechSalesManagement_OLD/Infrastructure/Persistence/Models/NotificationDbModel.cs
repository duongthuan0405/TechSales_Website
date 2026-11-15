using System;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class NotificationDbModel
{
    public Guid id { get; set; }
    public Guid user_id { get; set; }
    public string? title { get; set; }
    public string? content { get; set; }
    public bool is_read { get; set; }
    public Guid? ref_to { get; set; }
    public DateTimeOffset created_at { get; set; }

    // Navigation properties
    public UserDbModel user { get; set; } = null!;
}
