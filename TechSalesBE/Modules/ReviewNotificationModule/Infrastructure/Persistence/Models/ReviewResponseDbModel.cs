namespace ReviewNotificationModule.Infrastructure.Persistence.Models;

internal sealed class ReviewResponseDbModel
{
    public Guid Id { get; set; }
    public Guid ReviewId { get; set; }
    public Guid UserId { get; set; }
    public string? Content { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ReviewDbModel Review { get; set; } = null!;
}
