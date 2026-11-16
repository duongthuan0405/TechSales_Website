using PromotionModule.Domain.Enums;

namespace PromotionModule.Infrastructure.Persistence.Models;

internal sealed class VoucherDbModel
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public VoucherType Type { get; set; }
    public long? Value { get; set; }
    public int? MaxUsage { get; set; }
    public int? UsedCount { get; set; }
    public long? MinOrderAmount { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
