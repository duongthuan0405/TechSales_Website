using CommonModule.BusinessExceptions;
using MediatR;
using PaymentRefundModule.Application.Exceptions;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Application.UseCases.GetPayments;

internal sealed class GetPayments_UCHandler : IRequestHandler<GetPayments_UCInput, GetPayments_UCOutput>
{
    private readonly IPaymentRepository _payments;
    private readonly PaymentOwnershipService _ownership;

    public GetPayments_UCHandler(IPaymentRepository payments, PaymentOwnershipService ownership)
    {
        _payments = payments;
        _ownership = ownership;
    }

    public async Task<GetPayments_UCOutput> Handle(GetPayments_UCInput request, CancellationToken cancellationToken)
    {
        PaymentType? type = PaymentInputParser.OptionalType(request.Type);
        TransactionStatus? status = PaymentInputParser.OptionalStatus(request.Status);

        List<Payment> payments = await _payments.GetAllAsync(cancellationToken);
        List<GetPayments_UCOutput.PaymentInfo> items = new List<GetPayments_UCOutput.PaymentInfo>();
        foreach (Payment payment in payments)
        {
            if (type.HasValue && payment.Type != type.Value)
            {
                continue;
            }
            if (status.HasValue && payment.Status != status.Value)
            {
                continue;
            }
            Guid? refId = payment.OrderId;
            if (payment.Type == PaymentType.REPAIR)
            {
                refId = payment.RepairRequestId;
            }
            if (request.RefId.HasValue && refId != request.RefId.Value)
            {
                continue;
            }
            if (!await _ownership.IsOwnedAsync(payment, request.CustomerId, cancellationToken))
            {
                continue;
            }
            items.Add(new GetPayments_UCOutput.PaymentInfo(payment.Id, payment.Type.ToString(), payment.OrderId, payment.RepairRequestId, payment.TransactionMethodId, payment.Status.ToString(), payment.Amount, payment.TransactionRef, payment.FailureReason, payment.CompletedAt, payment.CreatedAt, payment.UpdatedAt));
        }

        int totalCount = items.Count;
        List<GetPayments_UCOutput.PaymentInfo> page = items.OrderByDescending(item => item.CreatedAt)
            .ThenBy(item => item.Id).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList();
        return new GetPayments_UCOutput(page, totalCount, request.PageNumber, request.PageSize);
    }
}
