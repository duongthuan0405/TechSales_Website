using MediatR;
using PromotionModule.Application.Repositories;
using PromotionModule.Domain.Entities;
using CommonModule.BusinessExceptions;
using PromotionModule.Application.Exceptions;
using PromotionModule.Application.Services;
using PromotionModule.Domain.Rules;

namespace PromotionModule.Application.UseCases.UpdateVoucher;

internal sealed class UpdateVoucher_UCHandler : IRequestHandler<UpdateVoucher_UCInput, UpdateVoucher_UCOutput>
{
    private readonly IVoucherRepository _vouchers;
    private readonly IExecuteAtomically _transaction;

    public UpdateVoucher_UCHandler(IVoucherRepository vouchers, IExecuteAtomically transaction)
    {
        _vouchers = vouchers;
        _transaction = transaction;
    }

    public async Task<UpdateVoucher_UCOutput> Handle(UpdateVoucher_UCInput input, CancellationToken cancellationToken)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            Voucher? voucher = await _vouchers.GetByIdAsync(input.Id, true, cancellationToken);
            if (voucher == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.VoucherNotFound);
            }

            string normalizedCode = VoucherRules.NormalizeCode(input.Code);
            Voucher? existing = await _vouchers.GetByCodeAsync(normalizedCode, cancellationToken);
            if (!VoucherRules.CodeAvailable(existing, voucher.Id))
            {
                throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.DuplicateCode);
            }

            if (!VoucherRules.CoversUsedCount(input.MaxUsage, voucher.UsedCount))
            {
                throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.UsageLimitTooSmall);
            }

            voucher.Update(input.Code, input.Type, input.Value, input.MaxUsage, input.MinOrderAmount, input.StartDate, input.EndDate, DateTimeOffset.UtcNow);
            await _vouchers.SaveAsync(voucher, cancellationToken);
            return new UpdateVoucher_UCOutput(voucher.Id, voucher.Code, voucher.Type.ToString(), voucher.Value, voucher.MaxUsage, voucher.UsedCount,
                voucher.MinOrderAmount, voucher.StartDate, voucher.EndDate, voucher.IsActive, voucher.CreatedAt, voucher.UpdatedAt);
        }, cancellationToken);
    }
}
