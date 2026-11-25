namespace TechSalesManagement.Application.Services.Strategies.VoucherStrategies;

public class PercentDiscountStrategy : IDiscountStrategy
{
    public decimal CalculateDiscount(decimal totalAmount, decimal voucherValue)
    {
        return totalAmount * (voucherValue / 100m);
    }
}
