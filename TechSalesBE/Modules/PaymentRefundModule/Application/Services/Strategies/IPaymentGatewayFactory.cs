namespace PaymentRefundModule.Application.Services.Strategies;

internal interface IPaymentGatewayFactory
{
    IPaymentGatewayStrategy GetStrategy(Guid transactionMethodId);
}
