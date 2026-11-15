using System;
using System.Threading.Tasks;
using TechSalesManagement.Application.Interfaces;

namespace TechSalesManagement.Infrastructure.Services;

public class PaymentGatewayService : IPaymentGatewayService
{
    public async Task<bool> RefundAsync(string transactionRef, decimal amount)
    {
        // Mocking external API call
        await Task.Delay(500);
        return true;
    }
}
