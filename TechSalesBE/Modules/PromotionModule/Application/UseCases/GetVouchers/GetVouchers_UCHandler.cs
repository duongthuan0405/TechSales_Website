using MediatR;
using PromotionModule.Application.Repositories;
using PromotionModule.Domain.Entities;

namespace PromotionModule.Application.UseCases.GetVouchers;

internal sealed class GetVouchers_UCHandler : IRequestHandler<GetVouchers_UCInput, GetVouchers_UCOutput>
{
    private readonly IVoucherRepository _vouchers;

    public GetVouchers_UCHandler(IVoucherRepository vouchers)
    {
        _vouchers = vouchers;
    }

    public async Task<GetVouchers_UCOutput> Handle(GetVouchers_UCInput input, CancellationToken cancellationToken)
    {
        (List<Voucher> vouchers, int totalCount) = await _vouchers.GetPageAsync(input.PageNumber, input.PageSize, cancellationToken);
        List<GetVouchers_UCOutput.VoucherItem> items = new List<GetVouchers_UCOutput.VoucherItem>();
        foreach (Voucher voucher in vouchers)
        {
            items.Add(new GetVouchers_UCOutput.VoucherItem(voucher.Id, voucher.Code, voucher.Type.ToString(), voucher.Value, voucher.MaxUsage, voucher.UsedCount,
                voucher.MinOrderAmount, voucher.StartDate, voucher.EndDate, voucher.IsActive, voucher.CreatedAt, voucher.UpdatedAt));
        }

        return new GetVouchers_UCOutput(items, totalCount, input.PageNumber, input.PageSize);
    }
}
