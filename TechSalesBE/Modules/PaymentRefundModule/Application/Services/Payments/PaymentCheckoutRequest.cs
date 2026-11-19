namespace PaymentRefundModule.Application.Services.Payments;

internal sealed record PaymentCheckoutRequest(Guid PaymentId, long Amount);
