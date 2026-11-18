namespace PromotionModule.Application.UseCases.UpdateVoucher;

internal sealed record UpdateVoucher_UCOutput(Guid Id, string Code, string Type, long? Value, int? MaxUsage, int UsedCount,
    long? MinOrderAmount, DateTimeOffset? StartDate, DateTimeOffset? EndDate, bool IsActive,
    DateTimeOffset? CreatedAt, DateTimeOffset? UpdatedAt);
