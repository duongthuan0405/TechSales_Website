namespace PaymentRefundModule.Application.Services.Strategies;

internal interface PaymentGatewayStrategy
{
    public string GatewayCode { get; }

    public abstract Task<PaymentCheckoutResult> CheckoutAsync(
        PaymentCheckoutRequest request, CancellationToken cancellationToken = default);
}
