namespace PaymentRefundModule.Application.Services.Strategies;

internal interface IPaymentGatewayStrategy
{
    string GatewayCode { get; }

    Task<PaymentCheckoutResult> CheckoutAsync(
        PaymentCheckoutRequest request, CancellationToken cancellationToken = default);
}
