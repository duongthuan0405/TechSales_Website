using MediatR;
using PromotionModule.Application.Repositories;
using PromotionModule.Domain.Entities;

namespace PromotionModule.Application.UseCases.GetVouchersForCustomer;

internal sealed class GetVouchersForCustomer_UCHandler : IRequestHandler<GetVouchersForCustomer_UCInput, GetVouchersForCustomer_UCOutput>
{
    private readonly IVoucherRepository _vouchers;

    public GetVouchersForCustomer_UCHandler(IVoucherRepository vouchers)
    {
        _vouchers = vouchers;
    }

    public async Task<GetVouchersForCustomer_UCOutput> Handle(GetVouchersForCustomer_UCInput input, CancellationToken cancellationToken)
    {
        (List<Voucher> vouchers, int totalCount) = await _vouchers.GetPageWithCanBeUsedAsync(input.PageNumber, input.PageSize, cancellationToken);
        List<GetVouchersForCustomer_UCOutput.VoucherItem> items = new List<GetVouchersForCustomer_UCOutput.VoucherItem>();
        foreach (Voucher voucher in vouchers)
        {
            items.Add(new GetVouchersForCustomer_UCOutput.VoucherItem(voucher.Id, voucher.Code, voucher.Type.ToString(), voucher.Value, voucher.MaxUsage, voucher.UsedCount,
                voucher.MinOrderAmount, voucher.StartDate, voucher.EndDate, voucher.IsActive, voucher.CreatedAt, voucher.UpdatedAt));
        }

        return new GetVouchersForCustomer_UCOutput(items, totalCount, input.PageNumber, input.PageSize);
    }
}
