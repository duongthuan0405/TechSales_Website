using MediatR;

namespace PaymentRefundModule.Application.UseCases.CheckoutPayment;

internal sealed record CheckoutPayment_UCInput(Guid CustomerId, Guid PaymentId) : IRequest<CheckoutPayment_UCOutput>;
