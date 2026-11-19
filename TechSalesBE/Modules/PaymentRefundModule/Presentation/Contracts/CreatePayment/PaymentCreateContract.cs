using MediatR;
using PaymentRefundModule.Application.UseCases.CreatePayment;
using PaymentRefundModule.Presentation.Contracts.CreatePayment;

namespace PaymentRefundModule.Presentation.Contracts.CreatePayment;

internal sealed class PaymentCreateContract : IPaymentCreateContract
{
    private readonly ISender _sender;

    public PaymentCreateContract(ISender sender)
    {
        _sender = sender;
    }

    public async Task<CreatePaymentResult> CreatePaymentAsync(Guid customerId, string paymentType, Guid refId,
        Guid transactionMethodId, CancellationToken cancellationToken = default)
    {
        CreatePayment_UCOutput output = await _sender.Send(
            new CreatePayment_UCInput(customerId, paymentType, refId, transactionMethodId), cancellationToken);
        return new CreatePaymentResult(output.Id);
    }
}
