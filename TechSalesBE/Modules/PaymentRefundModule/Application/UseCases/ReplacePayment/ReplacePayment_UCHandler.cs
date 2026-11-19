using MediatR;
using PaymentRefundModule.Application.Services;

namespace PaymentRefundModule.Application.UseCases.ReplacePayment;

internal sealed class ReplacePayment_UCHandler : IRequestHandler<ReplacePayment_UCInput, ReplacePayment_UCOutput>
{
    private readonly PaymentCreationService _creation;

    public ReplacePayment_UCHandler(PaymentCreationService creation)
    {
        _creation = creation;
    }

    public async Task<ReplacePayment_UCOutput> Handle(ReplacePayment_UCInput request, CancellationToken cancellationToken)
    {
        (Guid paymentId, Guid? replacedPaymentId) = await _creation.ExecuteAsync(
            request.CustomerId, request.Type, request.RefId, request.TransactionMethodId,
            replace: true, cancellationToken);
        return new ReplacePayment_UCOutput(paymentId, replacedPaymentId);
    }
}
