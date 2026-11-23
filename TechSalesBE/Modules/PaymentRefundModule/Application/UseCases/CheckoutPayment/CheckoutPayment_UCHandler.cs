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

    public CheckoutPayment_UCHandler(IPaymentRepository payments,
        PaymentOwnershipService ownership, IPaymentGatewayFactory gatewayFactory, IExecuteAtomically transaction)
    {
        _payments = payments;
        _ownership = ownership;
        _gatewayFactory = gatewayFactory;
        _transaction = transaction;
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

        TransactionMethod? method = payment.TransactionMethod;
        if (method == null)
        {
            throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.TransactionMethodNotFound);
        }
        PaymentRules.EnsureCheckoutAllowed(method.Type, payment.Status);

        if (!string.IsNullOrWhiteSpace(payment.PaymentUrl))
        {
            return new CheckoutPayment_UCOutput(payment.Id, payment.PaymentUrl);
        }

        IPaymentGatewayStrategy strategy = await _gatewayFactory.GetStrategyAsync(method.Id, cancellationToken);
        PaymentCheckoutRequest checkoutRequest = new PaymentCheckoutRequest(payment.Id, payment.Amount, request.ClientIpAddress);
        PaymentCheckoutResult result = await strategy.CheckoutAsync(checkoutRequest, cancellationToken);
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
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
