using MediatR;
using PaymentRefundModule.Application.Services;

namespace PaymentRefundModule.Application.UseCases.CreatePayment;

internal sealed class CreatePayment_UCHandler : IRequestHandler<CreatePayment_UCInput, CreatePayment_UCOutput>
{
    private readonly PaymentCreationService _creation;

    public CreatePayment_UCHandler(PaymentCreationService creation)
    {
        _creation = creation;
    }

    public async Task<CreatePayment_UCOutput> Handle(CreatePayment_UCInput request, CancellationToken cancellationToken)
    {
        var result = await _creation.ExecuteAsync(request.CustomerId, request.Type, request.RefId,
            request.TransactionMethodId, replace: false, cancellationToken);
        return new CreatePayment_UCOutput(result.PaymentId);
    }
}