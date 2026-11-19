namespace PaymentRefundModule.Application.UseCases.CheckoutPayment;

internal sealed record CheckoutPayment_UCOutput(Guid PaymentId, string PaymentUrl);
