namespace TechSalesManagement.Application.Services.Strategies.VoucherStrategies;

public interface IDiscountStrategy
{
    decimal CalculateDiscount(decimal totalAmount, decimal voucherValue);
}
