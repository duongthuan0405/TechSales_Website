using CommonModule.BusinessExceptions;
using MediatR;
using PaymentRefundModule.Application.Exceptions;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Application.Services.Payments;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Domain.Rules;

namespace PaymentRefundModule.Application.UseCases.CheckoutPayment;

internal sealed class CheckoutPayment_UCHandler : IRequestHandler<CheckoutPayment_UCInput, CheckoutPayment_UCOutput>
{
    private readonly IPaymentRepository _payments;
    private readonly PaymentOwnershipService _ownership;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly IExecuteAtomically _transaction;
    private readonly IPaymentLockService _paymentLock;

    public CheckoutPayment_UCHandler(IPaymentRepository payments,
        PaymentOwnershipService ownership, IPaymentGatewayFactory gatewayFactory, IExecuteAtomically transaction,
        IPaymentLockService paymentLock)
    {
        _payments = payments;
        _ownership = ownership;
        _gatewayFactory = gatewayFactory;
        _transaction = transaction;
        _paymentLock = paymentLock;
    }

    public async Task<CheckoutPayment_UCOutput> Handle(CheckoutPayment_UCInput request, CancellationToken cancellationToken)
    {
        Payment? payment = await _payments.GetByIdAsync(request.PaymentId, cancellationToken);
        if (payment == null)
        {
            throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.PaymentNotFound);
        }
        if (!await _ownership.IsOwnedAsync(payment, request.CustomerId, cancellationToken))
        {
            throw new BusinessException(BusinessExceptionCode.FORBIDDEN, BusinessExceptionMessage.PaymentNotOwned);
        }

        Guid referenceId = payment.OrderId ?? payment.RepairRequestId
            ?? throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.PaymentReferenceInvalid);
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
           
            if (!await _paymentLock.TryLockReferenceAsync(payment.Type, referenceId, cancellationToken))
                throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.CheckoutInProgress);

            Payment latest = await _payments.GetByIdAsync(payment.Id, cancellationToken)
                ?? throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.PaymentNotFound);
            TransactionMethod method = latest.TransactionMethod
                ?? throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.TransactionMethodNotFound);
            PaymentRules.EnsureCheckoutAllowed(method.Type, latest.Status);
            if (!string.IsNullOrWhiteSpace(latest.PaymentUrl))
                return new CheckoutPayment_UCOutput(latest.Id, latest.PaymentUrl);

            // Do not lock the payment row during the network call: callbacks can still update its status.
            IPaymentGatewayStrategy strategy = await _gatewayFactory.GetStrategyAsync(method.Id, cancellationToken);
            PaymentCheckoutResult result = await strategy.CheckoutAsync(
                new PaymentCheckoutRequest(latest.Id, latest.Amount, request.ClientIpAddress), cancellationToken);
            // Reload under lock so a callback received during checkout is not overwritten.
            Payment? current = await _payments.GetByIdForUpdateAsync(payment.Id, cancellationToken);
            if (current == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.PaymentNotFound);
            }

            current.SetPaymentUrl(result.PaymentUrl);
            await _payments.UpdateAsync(current, cancellationToken);
            return new CheckoutPayment_UCOutput(current.Id, current.PaymentUrl!);
        }, cancellationToken);
    }

}
