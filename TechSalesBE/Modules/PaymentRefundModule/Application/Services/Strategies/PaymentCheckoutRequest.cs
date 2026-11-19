namespace PaymentRefundModule.Application.Services.Strategies;

internal sealed record PaymentCheckoutRequest(Guid PaymentId, long Amount);
