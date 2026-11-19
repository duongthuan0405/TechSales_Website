using CommonModule.BusinessExceptions;
using MediatR;
using PaymentRefundModule.Application.Exceptions;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Domain.Entities;

namespace PaymentRefundModule.Application.UseCases.ConfirmCashPayment;

internal sealed class ConfirmCashPayment_UCHandler : IRequestHandler<ConfirmCashPayment_UCInput, ConfirmCashPayment_UCOutput>
{
    private readonly IPaymentRepository _payments;
    private readonly ITransactionMethodRepository _methods;
    private readonly IExecuteAtomically _transaction;

    public ConfirmCashPayment_UCHandler(IPaymentRepository payments, ITransactionMethodRepository methods, IExecuteAtomically transaction)
    {
        _payments = payments;
        _methods = methods;
        _transaction = transaction;
    }

    public Task<ConfirmCashPayment_UCOutput> Handle(ConfirmCashPayment_UCInput request, CancellationToken cancellationToken)
    {
        return _transaction.ExecuteAtomicallyAsync(async () =>
        {
            Payment? payment = await _payments.GetByIdForUpdateAsync(request.Id, cancellationToken);
            if (payment == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.PaymentNotFound);
            }

            TransactionMethod? method = await _methods.GetByIdAsync(payment.TransactionMethodId, cancellationToken);
            if (method == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.TransactionMethodNotFound);
            }

            bool changed = payment.ConfirmCash(method.Type);
            if (changed)
            {
                await _payments.UpdateAsync(payment, cancellationToken);
            }

            return new ConfirmCashPayment_UCOutput(payment.Id, payment.Status.ToString(), payment.CompletedAt, payment.UpdatedAt);
        }, cancellationToken);
    }
}
