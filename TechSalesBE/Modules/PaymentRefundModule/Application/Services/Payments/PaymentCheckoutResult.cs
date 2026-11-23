namespace PaymentRefundModule.Application.Services.Payments;

internal sealed record PaymentCheckoutResult(string PaymentUrl, DateTimeOffset? ExpiresAt = null);
