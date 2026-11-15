using System.Threading.Tasks;

namespace TechSalesManagement.Application.Services.Strategies.PaymentStrategies;

public interface IPaymentStrategyFactory
{
    IPaymentStrategy GetStrategy(string paymentMethodName);
}
