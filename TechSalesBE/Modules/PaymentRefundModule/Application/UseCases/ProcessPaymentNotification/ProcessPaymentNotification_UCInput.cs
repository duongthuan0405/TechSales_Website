using MediatR;

namespace PaymentRefundModule.Application.UseCases.ProcessPaymentNotification;

// Internal command only: never bind directly to an HTTP request.
internal sealed record ProcessPaymentNotification_UCInput(
    Guid PaymentId, long Amount, string GatewayCode, string TransactionRef,
    PaymentNotificationStatus Status) : IRequest<ProcessPaymentNotification_UCOutput>;
