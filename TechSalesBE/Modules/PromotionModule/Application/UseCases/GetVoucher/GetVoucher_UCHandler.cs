using MediatR;
using PromotionModule.Application.Repositories;
using PromotionModule.Domain.Entities;
using CommonModule.BusinessExceptions;
using PromotionModule.Application.Exceptions;

namespace PromotionModule.Application.UseCases.GetVoucher;

internal sealed class GetVoucherForCustomer_UCHandler : IRequestHandler<GetVoucherForCustomer_UCInput, GetVoucherForCustomer_UCOutput>
{
    private readonly IVoucherRepository _vouchers;

    public GetVoucherForCustomer_UCHandler(IVoucherRepository vouchers)
    {
        _vouchers = vouchers;
    }

    public async Task<GetVoucherForCustomer_UCOutput> Handle(GetVoucherForCustomer_UCInput input, CancellationToken cancellationToken)
    {
        Voucher? voucher = await _vouchers.GetByIdAsync(input.Id, false, cancellationToken);
        if (voucher == null)
        {
            throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.VoucherNotFound);
        }

        return new GetVoucherForCustomer_UCOutput(voucher.Id, voucher.Code, voucher.Type.ToString(), voucher.Value, voucher.MaxUsage, voucher.UsedCount,
                voucher.MinOrderAmount, voucher.StartDate, voucher.EndDate, voucher.IsActive, voucher.CreatedAt, voucher.UpdatedAt);
    }
}
