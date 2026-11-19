using MediatR;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;

namespace PaymentRefundModule.Application.UseCases.GetPaymentsForStaff;

internal sealed class GetPaymentsForStaff_UCHandler : IRequestHandler<GetPaymentsForStaff_UCInput, GetPaymentsForStaff_UCOutput>
{
    private readonly IPaymentRepository _payments;

    public GetPaymentsForStaff_UCHandler(IPaymentRepository payments)
    {
        _payments = payments;
    }

    public async Task<GetPaymentsForStaff_UCOutput> Handle(GetPaymentsForStaff_UCInput request, CancellationToken cancellationToken)
    {
        var page = await _payments.GetPageAsync(request.PageNumber, request.PageSize,
            PaymentInputParser.OptionalType(request.Type), PaymentInputParser.OptionalStatus(request.Status), request.RefId, cancellationToken);
        List<GetPaymentsForStaff_UCOutput.PaymentInfo> items = page.Items.Select(payment =>
            new GetPaymentsForStaff_UCOutput.PaymentInfo(payment.Id, payment.Type.ToString(), payment.OrderId,
                payment.RepairRequestId, payment.TransactionMethodId, payment.Status.ToString(), payment.Amount,
                payment.TransactionRef, payment.FailureReason, payment.CompletedAt, payment.CreatedAt, payment.UpdatedAt)).ToList();
        return new GetPaymentsForStaff_UCOutput(items, page.TotalCount, request.PageNumber, request.PageSize);
    }
}
