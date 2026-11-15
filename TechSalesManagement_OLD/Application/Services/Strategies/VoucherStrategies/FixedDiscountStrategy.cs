using System;

namespace TechSalesManagement.Application.Services.Strategies.VoucherStrategies;


public class FixedDiscountStrategy : IDiscountStrategy
{
    public decimal CalculateDiscount(decimal totalAmount, decimal voucherValue)
    {
        return Math.Min(voucherValue, totalAmount);
    }
}
