using MediatR;
using PromotionModule.Application.Repositories;
using PromotionModule.Domain.Entities;
using CommonModule.BusinessExceptions;
using PromotionModule.Application.Exceptions;
using PromotionModule.Application.Services;
using PromotionModule.Domain.Rules;

namespace PromotionModule.Application.UseCases.CreateVoucher;

internal sealed class CreateVoucher_UCHandler : IRequestHandler<CreateVoucher_UCInput, CreateVoucher_UCOutput>
{
    private readonly IVoucherRepository _vouchers;
    private readonly IExecuteAtomically _transaction;

    public CreateVoucher_UCHandler(IVoucherRepository vouchers, IExecuteAtomically transaction)
    {
        _vouchers = vouchers;
        _transaction = transaction;
    }

    public async Task<CreateVoucher_UCOutput> Handle(CreateVoucher_UCInput input, CancellationToken cancellationToken)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            string normalizedCode = VoucherRules.NormalizeCode(input.Code);
            Voucher? existing = await _vouchers.GetByCodeAsync(normalizedCode, cancellationToken);
            if (!VoucherRules.CodeAvailable(existing, null))
            {
                throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.DuplicateCode);
            }

            Voucher voucher = Voucher.Create(input.Code, input.Type, input.Value, input.MaxUsage, input.MinOrderAmount, input.StartDate, input.EndDate, DateTimeOffset.UtcNow);
            await _vouchers.SaveAsync(voucher, cancellationToken);
            return new CreateVoucher_UCOutput(voucher.Id, voucher.Code, voucher.Type.ToString(), voucher.Value, voucher.MaxUsage, voucher.UsedCount,
                voucher.MinOrderAmount, voucher.StartDate, voucher.EndDate, voucher.IsActive, voucher.CreatedAt, voucher.UpdatedAt);
        }, cancellationToken);
    }
}
