namespace PromotionModule.Application.UseCases.CreateVoucher;

internal sealed record CreateVoucher_UCOutput(Guid Id, string Code, string Type, long? Value, int? MaxUsage, int UsedCount,
    long? MinOrderAmount, DateTimeOffset? StartDate, DateTimeOffset? EndDate, bool IsActive,
    DateTimeOffset? CreatedAt, DateTimeOffset? UpdatedAt);
