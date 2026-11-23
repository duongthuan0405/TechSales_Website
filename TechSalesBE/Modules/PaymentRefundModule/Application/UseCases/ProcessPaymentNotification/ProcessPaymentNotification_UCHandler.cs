using CommonModule.BusinessExceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentRefundModule.Application.Exceptions;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Domain.Rules;

namespace PaymentRefundModule.Application.UseCases.ProcessPaymentNotification;

internal sealed class ProcessPaymentNotification_UCHandler : IRequestHandler<ProcessPaymentNotification_UCInput, ProcessPaymentNotification_UCOutput>
{
    private readonly IPaymentRepository _payments;
    private readonly IExecuteAtomically _transaction;
    private readonly ILogger<ProcessPaymentNotification_UCHandler> _logger;

    public ProcessPaymentNotification_UCHandler(IPaymentRepository payments,
        IExecuteAtomically transaction, ILogger<ProcessPaymentNotification_UCHandler> logger)
    {
        _payments = payments;
        _transaction = transaction;
        _logger = logger;
    }

    public Task<ProcessPaymentNotification_UCOutput> Handle(ProcessPaymentNotification_UCInput request, CancellationToken cancellationToken)
    {
        return _transaction.ExecuteAtomicallyAsync(async () =>
        {
            Payment? payment = await _payments.GetByIdForUpdateAsync(request.PaymentId, cancellationToken);
            if (payment == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.PaymentNotFound);
            }

            TransactionMethod? method = payment.TransactionMethod;
            if (method == null || !PaymentRules.MatchesNotification(
                method.Type, method.Code, request.GatewayCode, payment.Amount, request.Amount))
            {
                throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.PaymentNotificationInvalid);
            }

            if (request.Status == PaymentNotificationStatus.Failed)
            {
                bool failed = payment.FailOnline(request.TransactionRef);
                if (failed)
                {
                    await _payments.UpdateAsync(payment, cancellationToken);
                }
                return new ProcessPaymentNotification_UCOutput(failed);
            }

            if (request.Status != PaymentNotificationStatus.Success)
            {
                _logger.LogWarning("Payment notification left payment unchanged. PaymentId: {PaymentId}, GatewayCode: {GatewayCode}, Status: {Status}",
                    payment.Id, request.GatewayCode, request.Status);
                return new ProcessPaymentNotification_UCOutput(false);
            }

            bool changed = payment.ConfirmOnline(request.TransactionRef);
            if (changed)
            {
                await _payments.UpdateAsync(payment, cancellationToken);
            }
            return new ProcessPaymentNotification_UCOutput(changed);
        }, cancellationToken);
    }
}
