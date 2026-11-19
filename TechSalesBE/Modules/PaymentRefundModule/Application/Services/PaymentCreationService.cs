using CommonModule.BusinessExceptions;
using PaymentRefundModule.Application.Exceptions;
using PaymentRefundModule.Application.Ports;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Domain.Rules;

namespace PaymentRefundModule.Application.Services;

internal sealed class PaymentCreationService
{
    private readonly IPaymentRepository _payments;
    private readonly ITransactionMethodRepository _methods;
    private readonly PaymentOwnershipService _ownership;
    private readonly IPaymentAmountPort _amounts;
    private readonly IExecuteAtomically _transaction;

    public PaymentCreationService(IPaymentRepository payments, ITransactionMethodRepository methods,
        PaymentOwnershipService ownership, IPaymentAmountPort amounts, IExecuteAtomically transaction)
    {
        _payments = payments;
        _methods = methods;
        _ownership = ownership;
        _amounts = amounts;
        _transaction = transaction;
    }

    public Task<(Guid PaymentId, Guid? ReplacedPaymentId)> ExecuteAsync(Guid customerId, string? paymentType,
        Guid refId, Guid transactionMethodId, bool replace, CancellationToken cancellationToken)
    {
        PaymentType type = PaymentInputParser.RequiredType(paymentType);
        return _transaction.ExecuteAtomicallyAsync(async () =>
        {
            await _payments.LockReferenceAsync(type, refId, cancellationToken);
            if (!await _ownership.IsOwnedAsync(type, refId, customerId, cancellationToken))
            {
                throw new BusinessException(BusinessExceptionCode.FORBIDDEN, BusinessExceptionMessage.PaymentNotOwned);
            }

            List<Payment> history = await _payments.GetByRefIdAsync(refId, type, cancellationToken);
            List<Payment> pending = new List<Payment>();
            foreach (Payment item in history)
            {
                Payment? current = await _payments.GetByIdForUpdateAsync(item.Id, cancellationToken);
                if (current == null)
                {
                    continue;
                }
                if (PaymentRules.IsPaid(current.Status))
                {
                    throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.PaymentAlreadyPaid);
                }
                if (PaymentRules.IsPending(current.Status))
                {
                    pending.Add(current);
                }
            }

            if (pending.Count > 1)
            {
                throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.MultiplePendingPayments);
            }
            if (!replace && pending.Count > 0) // Thao tác tạo mới thì mới check cái này
            {
                throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.PendingPaymentExists);
            }

            TransactionMethod? method = await _methods.GetByIdAsync(transactionMethodId, cancellationToken);
            if (method == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.TransactionMethodNotFound);
            }

            long amount = await _amounts.GetAmountAsync(type, refId, cancellationToken);
            if (!PaymentRules.HasValidAmount(amount))
            {
                throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.PaymentAmountInvalid);
            }

            Guid? replacedId = null;
            if (pending.Count == 1)
            {
                Payment previous = pending[0];
                TransactionMethod? previousMethod = await _methods.GetByIdAsync(previous.TransactionMethodId, cancellationToken);
                if (previousMethod == null)
                {
                    throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.TransactionMethodNotFound);
                }
                if (previousMethod.Type != TransactionMethodType.CASH)
                {
                    //throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.OnlineReplacementUnavailable);
                }

                previous.CancelForReplacement(previousMethod.Type);
                await _payments.UpdateAsync(previous, cancellationToken);
                replacedId = previous.Id;
            }

            Payment payment = new Payment(type, refId, method.Id, amount, method);
            await _payments.AddAsync(payment, cancellationToken);
            return (payment.Id, replacedId);
        }, cancellationToken);
    }
}
