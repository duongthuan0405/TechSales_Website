using System;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class UserProfileDbModel
{
    public Guid user_id { get; set; }
    public string full_name { get; set; } = string.Empty;
    public string phone { get; set; } = string.Empty;
    public string? avatar_url { get; set; }
    public DateTimeOffset? date_of_birth { get; set; }
    public DateTimeOffset created_at { get; set; }
    public DateTimeOffset? updated_at { get; set; }

    // Navigation properties
    public UserDbModel user { get; set; } = null!;
}
