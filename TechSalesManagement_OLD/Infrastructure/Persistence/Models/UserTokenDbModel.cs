using System;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class UserTokenDbModel
{
    public Guid id { get; set; }
    public Guid user_id { get; set; }
    public string token { get; set; } = string.Empty;
    public TokenType type { get; set; }
    public DateTimeOffset expired_at { get; set; }
    public DateTimeOffset? used_at { get; set; }
    public DateTimeOffset created_at { get; set; }

    // Navigation properties
    public UserDbModel user { get; set; } = null!;
}
