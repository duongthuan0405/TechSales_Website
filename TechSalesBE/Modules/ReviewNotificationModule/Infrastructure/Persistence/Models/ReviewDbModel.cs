using ReviewNotificationModule.Domain.Enums;

namespace ReviewNotificationModule.Infrastructure.Persistence.Models;

internal sealed class ReviewDbModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public ReviewStatus? Status { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<ReviewResponseDbModel> Responses { get; set; } = new HashSet<ReviewResponseDbModel>();
}
