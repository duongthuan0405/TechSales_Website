namespace PromotionModule.Application.UseCases.GetVoucher;

internal sealed record GetVoucherForCustomer_UCOutput(Guid Id, string Code, string Type, long? Value, int? MaxUsage, int UsedCount,
    long? MinOrderAmount, DateTimeOffset? StartDate, DateTimeOffset? EndDate, bool IsActive,
    DateTimeOffset? CreatedAt, DateTimeOffset? UpdatedAt);
