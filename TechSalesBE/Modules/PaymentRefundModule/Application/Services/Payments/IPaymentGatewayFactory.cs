namespace PaymentRefundModule.Application.Services.Payments;

internal interface IPaymentGatewayFactory
{
    Task<IPaymentGatewayStrategy> GetStrategyAsync(Guid transactionMethodId, CancellationToken cancellationToken = default);
}
