using System;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class ReviewResponseDbModel
{
    public Guid id { get; set; }
    public Guid review_id { get; set; }
    public Guid user_id { get; set; }
    public string? content { get; set; }
    public DateTimeOffset? created_at { get; set; }
    public DateTimeOffset? updated_at { get; set; }

    // Navigation properties
    public ReviewDbModel review { get; set; } = null!;
    public UserDbModel user { get; set; } = null!;
}
