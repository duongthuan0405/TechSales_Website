using MediatR;

namespace PaymentRefundModule.Application.UseCases.CheckoutPayment;

internal sealed record CheckoutPayment_UCInput(Guid CustomerId, Guid PaymentId, string? ClientIpAddress = null) : IRequest<CheckoutPayment_UCOutput>;
