using MediatR;
using PromotionModule.Application.Repositories;
using PromotionModule.Domain.Entities;
using CommonModule.BusinessExceptions;
using PromotionModule.Application.Exceptions;
using PromotionModule.Application.Services;

namespace PromotionModule.Application.UseCases.DeactivateVoucher;

internal sealed class DeactivateVoucher_UCHandler : IRequestHandler<DeactivateVoucher_UCInput, DeactivateVoucher_UCOutput>
{
    private readonly IVoucherRepository _vouchers;
    private readonly IExecuteAtomically _transaction;

    public DeactivateVoucher_UCHandler(IVoucherRepository vouchers, IExecuteAtomically transaction)
    {
        _vouchers = vouchers;
        _transaction = transaction;
    }

    public async Task<DeactivateVoucher_UCOutput> Handle(DeactivateVoucher_UCInput input, CancellationToken cancellationToken)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            Voucher? voucher = await _vouchers.GetByIdAsync(input.Id, true, cancellationToken);
            if (voucher == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.VoucherNotFound);
            }

            voucher.Deactivate(DateTimeOffset.UtcNow);
            await _vouchers.SaveAsync(voucher, cancellationToken);
            return new DeactivateVoucher_UCOutput(voucher.Id, voucher.IsActive);
        }, cancellationToken);
    }
}
