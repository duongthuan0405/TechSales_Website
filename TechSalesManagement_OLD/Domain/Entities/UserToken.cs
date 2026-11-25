using System;
using TechSalesManagement.Domain.Common;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Domain.Entities;

public class UserToken
{
    public Guid id { get; set; }
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? updatedAt { get; set; }

    public Guid userId { get; set; }
    public string token { get; set; } = string.Empty;
    public TokenType type { get; set; }
    public DateTimeOffset expiredAt { get; set; }
    public DateTimeOffset? usedAt { get; set; }

    public UserToken(Guid userId, string token, TokenType type, DateTimeOffset expiredAt)
    {
        this.userId = userId;
        this.token = token;
        this.type = type;
        this.expiredAt = expiredAt;
    }

    public UserToken() { }
}
