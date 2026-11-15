using System;
using System.Collections.Generic;
using System.Linq;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Services.Strategies.VoucherStrategies;


public class DiscountStrategyFactory : IDiscountStrategyFactory
{
    private readonly IEnumerable<IDiscountStrategy> _strategies;

    public DiscountStrategyFactory(IEnumerable<IDiscountStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IDiscountStrategy GetStrategy(VoucherType voucherType)
    {
        return voucherType switch
        {
            VoucherType.FIXED => _strategies.FirstOrDefault(s => s is FixedDiscountStrategy) 
                ?? throw new InvalidOperationException("FixedDiscountStrategy not registered"),
            VoucherType.PERCENT => _strategies.FirstOrDefault(s => s is PercentDiscountStrategy)
                ?? throw new InvalidOperationException("PercentDiscountStrategy not registered"),
            _ => throw new ArgumentException("Invalid voucher type", nameof(voucherType))
        };
    }
}
