using System;
using System.Threading.Tasks;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Services.Strategies.Refund;

public class VnPayRefundStrategy : IRefundStrategy
{
    private readonly IPaymentGatewayService _paymentGateway;

    public VnPayRefundStrategy(IPaymentGatewayService paymentGateway)
    {
        _paymentGateway = paymentGateway;
    }

    public PaymentMethodType MethodType => PaymentMethodType.ONLINE;

    public async Task<bool> ExecuteRefundAsync(Payment payment)
    {
        if (string.IsNullOrEmpty(payment.transactionRef)) return false;
        return await _paymentGateway.RefundAsync(payment.transactionRef, payment.amount);
    }
}
