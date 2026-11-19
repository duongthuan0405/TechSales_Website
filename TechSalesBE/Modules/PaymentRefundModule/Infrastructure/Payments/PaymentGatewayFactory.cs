using CommonModule.BusinessExceptions;
using PaymentRefundModule.Application.Exceptions;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services.Payments;
using PaymentRefundModule.Domain.Entities;

namespace PaymentRefundModule.Infrastructure.Payments;

internal sealed class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly Dictionary<string, IPaymentGatewayStrategy> _strategies;
    private readonly ITransactionMethodRepository _methods;

    public PaymentGatewayFactory(ITransactionMethodRepository methods, IEnumerable<IPaymentGatewayStrategy> strategies)
    {
        _methods = methods;
        _strategies = strategies.ToDictionary(strategy => strategy.GatewayCode, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IPaymentGatewayStrategy> GetStrategyAsync(Guid transactionMethodId, CancellationToken cancellationToken = default)
    {
        TransactionMethod? method = await _methods.GetByIdAsync(transactionMethodId, cancellationToken);
        if (method == null)
        {
            throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.TransactionMethodNotFound);
        }

        string? gatewayCode = method.Code;
        if (string.IsNullOrWhiteSpace(gatewayCode)
            || !_strategies.TryGetValue(gatewayCode.Trim(), out IPaymentGatewayStrategy? strategy))
        {
            throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.PaymentGatewayNotConfigured);
        }

        return strategy;
    }
}
