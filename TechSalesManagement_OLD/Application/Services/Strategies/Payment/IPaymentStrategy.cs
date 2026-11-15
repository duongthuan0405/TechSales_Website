using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Strategies.PaymentStrategies;

public class PaymentResult
{
    public bool IsSuccess { get; set; }
    public string? CheckoutUrl { get; set; }
    public string Message { get; set; } = string.Empty;
}

public interface IPaymentStrategy
{
    Task<PaymentResult> ProcessPaymentAsync(Order order);
}
