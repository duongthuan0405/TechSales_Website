using System;
using TechSalesManagement.Domain.Common;

namespace TechSalesManagement.Domain.Entities;

public class UserProfile
{
    public Guid id { get; set; }
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? updatedAt { get; set; }

    public Guid userId { get; set; }
    public string fullName { get; set; } = string.Empty;
    public string phone { get; set; } = string.Empty;
    public string? avatarUrl { get; set; }
    public DateTimeOffset? dateOfBirth { get; set; }

    public UserProfile(Guid userId, string fullName, string phone)
    {
        this.userId = userId;
        this.fullName = fullName;
        this.phone = phone;
    }

    public UserProfile() { }
}
