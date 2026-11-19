using PaymentRefundModule.Application.Services.Strategies;

namespace PaymentRefundModule.Infrastructure.Services.Strategies;

internal sealed class MomoPaymentStrategy : IPaymentGatewayStrategy
{
    public string GatewayCode => "MOMO";

    public Task<PaymentCheckoutResult> CheckoutAsync(
        PaymentCheckoutRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
