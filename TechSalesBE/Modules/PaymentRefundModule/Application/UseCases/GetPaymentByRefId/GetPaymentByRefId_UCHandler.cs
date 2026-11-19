using CommonModule.BusinessExceptions;
using MediatR;
using PaymentRefundModule.Application.Exceptions;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Application.UseCases.GetPaymentByRefId;

internal sealed class GetPaymentByRefId_UCHandler : IRequestHandler<GetPaymentByRefId_UCInput, GetPaymentByRefId_UCOutput>
{
    private readonly IPaymentRepository _payments;
    private readonly PaymentOwnershipService _ownership;

    public GetPaymentByRefId_UCHandler(IPaymentRepository payments, PaymentOwnershipService ownership)
    {
        _payments = payments;
        _ownership = ownership;
    }

    public async Task<GetPaymentByRefId_UCOutput> Handle(GetPaymentByRefId_UCInput request, CancellationToken cancellationToken)
    {
        PaymentType type = PaymentInputParser.RequiredType(request.Type);
        TransactionStatus? status = PaymentInputParser.OptionalStatus(request.Status);

        if (!request.CanViewAll && !await _ownership.IsOwnedAsync(type, request.RefId, request.CustomerId, cancellationToken))
        {
            throw new BusinessException(BusinessExceptionCode.FORBIDDEN, BusinessExceptionMessage.PaymentAccessDenied);
        }
        var page = await _payments.GetPageAsync(request.PageNumber, request.PageSize, type,
            status, request.RefId, cancellationToken);
        List<GetPaymentByRefId_UCOutput.PaymentInfo> items = new List<GetPaymentByRefId_UCOutput.PaymentInfo>();
        foreach (Payment payment in page.Items)
        {

            items.Add(new GetPaymentByRefId_UCOutput.PaymentInfo(payment.Id, payment.Type.ToString(), payment.OrderId, payment.RepairRequestId, payment.TransactionMethodId, payment.Status.ToString(), payment.Amount, payment.TransactionRef, payment.FailureReason, payment.CompletedAt, payment.CreatedAt, payment.UpdatedAt));
        }

        return new GetPaymentByRefId_UCOutput(items, page.TotalCount, request.PageNumber, request.PageSize);
    }
}
