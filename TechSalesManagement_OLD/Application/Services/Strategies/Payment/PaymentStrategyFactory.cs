using System;
using System.Collections.Generic;
using TechSalesManagement.Application.Exceptions;

namespace TechSalesManagement.Application.Services.Strategies.PaymentStrategies;

public class PaymentStrategyFactory : IPaymentStrategyFactory
{
    private readonly IEnumerable<IPaymentStrategy> _strategies;

    public PaymentStrategyFactory(IEnumerable<IPaymentStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IPaymentStrategy GetStrategy(string paymentMethodName)
    {
        if (!Enum.TryParse<PaymentProvider>(paymentMethodName.ToUpper(), true, out var provider))
        {
            throw new BadRequestException($"Payment method '{paymentMethodName}' is not supported.");
        }

        var strategyType = provider switch
        {
            PaymentProvider.MOMO => typeof(MomoPaymentStrategy),
            PaymentProvider.VNPAY => typeof(VnPayPaymentStrategy),
            PaymentProvider.COD => typeof(CodPaymentStrategy),
            _ => throw new BadRequestException($"Payment method '{paymentMethodName}' is not supported.")
        };

        foreach (var strategy in _strategies)
        {
            if (strategy.GetType() == strategyType)
            {
                return strategy;
            }
        }

        throw new InvalidOperationException($"Strategy for {paymentMethodName} is not registered.");
    }
}
