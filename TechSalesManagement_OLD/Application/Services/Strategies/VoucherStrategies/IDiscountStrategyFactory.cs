using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Services.Strategies.VoucherStrategies;

public interface IDiscountStrategyFactory
{
    IDiscountStrategy GetStrategy(VoucherType voucherType);
}
