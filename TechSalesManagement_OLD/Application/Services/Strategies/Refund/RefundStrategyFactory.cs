using System;
using System.Collections.Generic;
using System.Linq;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Services.Strategies.Refund;

public interface IRefundStrategyFactory
{
    IRefundStrategy GetStrategy(PaymentMethodType methodType);
}

public class RefundStrategyFactory : IRefundStrategyFactory
{
    private readonly IEnumerable<IRefundStrategy> _strategies;

    public RefundStrategyFactory(IEnumerable<IRefundStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IRefundStrategy GetStrategy(PaymentMethodType methodType)
    {
        var strategy = _strategies.FirstOrDefault(s => s.MethodType == methodType);
        if (strategy == null)
        {
            throw new ArgumentException($"No refund strategy found for payment method: {methodType}");
        }
        return strategy;
    }
}
