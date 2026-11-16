namespace ReviewNotificationModule.Infrastructure.Persistence.Models;

internal sealed class NotificationDbModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public bool? IsRead { get; set; }
    public Guid? RefTo { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}
