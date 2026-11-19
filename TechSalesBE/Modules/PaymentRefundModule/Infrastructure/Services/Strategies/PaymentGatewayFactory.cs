using CommonModule.BusinessExceptions;
using Microsoft.Extensions.Configuration;
using PaymentRefundModule.Application.Exceptions;
using PaymentRefundModule.Application.Services.Strategies;

namespace PaymentRefundModule.Infrastructure.Services.Strategies;

internal sealed class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, IPaymentGatewayStrategy> _strategies;

    public PaymentGatewayFactory(IConfiguration configuration, IEnumerable<IPaymentGatewayStrategy> strategies)
    {
        _configuration = configuration;
        _strategies = strategies.ToDictionary(strategy => strategy.GatewayCode, StringComparer.OrdinalIgnoreCase);
    }

    public IPaymentGatewayStrategy GetStrategy(Guid transactionMethodId)
    {
        string? gatewayCode = _configuration[$"PaymentRefund:GatewayMethods:{transactionMethodId:D}"];
        if (string.IsNullOrWhiteSpace(gatewayCode)
            || !_strategies.TryGetValue(gatewayCode.Trim(), out IPaymentGatewayStrategy? strategy))
        {
            throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.PaymentGatewayNotConfigured);
        }

        return strategy;
    }
}
