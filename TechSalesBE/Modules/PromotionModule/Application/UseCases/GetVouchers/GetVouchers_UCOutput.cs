namespace PromotionModule.Application.UseCases.GetVouchers;

internal sealed record GetVouchers_UCOutput(List<GetVouchers_UCOutput.VoucherItem> Items, int TotalCount, int PageNumber, int PageSize)
{
    internal sealed record VoucherItem(Guid Id, string Code, string Type, long? Value, int? MaxUsage, int UsedCount,
    long? MinOrderAmount, DateTimeOffset? StartDate, DateTimeOffset? EndDate, bool IsActive,
    DateTimeOffset? CreatedAt, DateTimeOffset? UpdatedAt);
}
