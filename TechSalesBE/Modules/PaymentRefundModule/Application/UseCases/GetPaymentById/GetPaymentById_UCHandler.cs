using CommonModule.BusinessExceptions;
using MediatR;
using PaymentRefundModule.Application.Exceptions;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Domain.Entities;

namespace PaymentRefundModule.Application.UseCases.GetPaymentById;

internal sealed class GetPaymentById_UCHandler : IRequestHandler<GetPaymentById_UCInput, GetPaymentById_UCOutput>
{
    private readonly IPaymentRepository _payments;
    private readonly PaymentOwnershipService _ownership;

    public GetPaymentById_UCHandler(IPaymentRepository payments, PaymentOwnershipService ownership)
    {
        _payments = payments;
        _ownership = ownership;
    }

    public async Task<GetPaymentById_UCOutput> Handle(GetPaymentById_UCInput request, CancellationToken cancellationToken)
    {
        Payment? payment = await _payments.GetByIdAsync(request.Id, cancellationToken);
        if (payment == null)
        {
            throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.PaymentNotFound);
        }

        if (!request.CanViewAll && !await _ownership.IsOwnedAsync(payment, request.CustomerId, cancellationToken))
        {
            throw new BusinessException(BusinessExceptionCode.FORBIDDEN, BusinessExceptionMessage.PaymentAccessDenied);
        }

        return new GetPaymentById_UCOutput(payment.Id, payment.Type.ToString(), payment.OrderId, payment.RepairRequestId, payment.TransactionMethodId, payment.Status.ToString(), payment.Amount, payment.TransactionRef, payment.FailureReason, payment.CompletedAt, payment.CreatedAt, payment.UpdatedAt);
    }
}
