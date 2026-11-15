using System;
using TechSalesManagement.Domain.Common;

namespace TechSalesManagement.Domain.Entities;

public class Notification
{
    public Guid id { get; set; }
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? updatedAt { get; set; }

    public Guid userId { get; set; }
    public string title { get; set; } = string.Empty;
    public string content { get; set; } = string.Empty;
    public bool isRead { get; set; }
    public Guid? refTo { get; set; }

    public Notification(Guid userId, string title, string content)
    {
        this.userId = userId;
        this.title = title;
        this.content = content;
        isRead = false;
    }

    public Notification() { }
}
