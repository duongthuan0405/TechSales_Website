using MediatR;

namespace PaymentRefundModule.Application.UseCases.ReplacePayment;

internal sealed record ReplacePayment_UCInput(Guid CustomerId, string? Type, Guid RefId,
    Guid TransactionMethodId) : IRequest<ReplacePayment_UCOutput>;
