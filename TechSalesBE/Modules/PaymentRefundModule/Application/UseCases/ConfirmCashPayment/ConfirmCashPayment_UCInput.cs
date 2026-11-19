using MediatR;

namespace PaymentRefundModule.Application.UseCases.ConfirmCashPayment;

internal sealed record ConfirmCashPayment_UCInput(Guid Id) : IRequest<ConfirmCashPayment_UCOutput>;
