using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Strategies.PaymentStrategies;

public class CodPaymentStrategy : IPaymentStrategy
{
    public Task<PaymentResult> ProcessPaymentAsync(Order order)
    {
        return Task.FromResult(new PaymentResult
        {
            IsSuccess = true,
            CheckoutUrl = null,
            Message = "Order placed successfully. Payment will be collected on delivery."
        });
    }
}
