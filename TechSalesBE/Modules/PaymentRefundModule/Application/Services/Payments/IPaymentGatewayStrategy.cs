namespace PaymentRefundModule.Application.Services.Payments;

internal interface IPaymentGatewayStrategy
{
    string GatewayCode { get; }

    Task<PaymentCheckoutResult> CheckoutAsync(
        PaymentCheckoutRequest request, CancellationToken cancellationToken = default);
}
