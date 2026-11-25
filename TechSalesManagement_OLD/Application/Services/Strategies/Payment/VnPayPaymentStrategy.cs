using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Strategies.PaymentStrategies;

public class VnPayPaymentStrategy : IPaymentStrategy
{
    public Task<PaymentResult> ProcessPaymentAsync(Order order)
    {
        // Mock VNPay payment gateway URL generation
        var mockUrl = $"https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_TxnRef={order.id}&vnp_Amount={(long)(order.totalAmount * 100)}";

        return Task.FromResult(new PaymentResult
        {
            IsSuccess = true,
            CheckoutUrl = mockUrl,
            Message = "Redirecting to VNPay Payment Gateway..."
        });
    }
}
