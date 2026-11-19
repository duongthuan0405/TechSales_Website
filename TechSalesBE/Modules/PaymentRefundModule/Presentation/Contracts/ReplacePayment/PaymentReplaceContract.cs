using MediatR;
using PaymentRefundModule.Application.UseCases.ReplacePayment;
using PaymentRefundModule.Presentation.Contracts.ReplacePayment;

namespace PaymentRefundModule.Presentation.Contracts.ReplacePayment;

internal sealed class PaymentReplaceContract : IPaymentReplaceContract
{
    private readonly ISender _sender;

    public PaymentReplaceContract(ISender sender)
    {
        _sender = sender;
    }

    public async Task<ReplacePaymentResult> ReplacePaymentAsync(Guid customerId, string paymentType, Guid refId,
        Guid transactionMethodId, CancellationToken cancellationToken = default)
    {
        ReplacePayment_UCInput input = new ReplacePayment_UCInput(customerId, paymentType, refId, transactionMethodId);
        ReplacePayment_UCOutput output = await _sender.Send(input, cancellationToken);
        return new ReplacePaymentResult(output.PaymentId, output.ReplacedPaymentId);
    }
}
