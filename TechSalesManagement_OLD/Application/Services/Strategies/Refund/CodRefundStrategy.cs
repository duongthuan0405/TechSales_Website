using System;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Services.Strategies.Refund;

public class CodRefundStrategy : IRefundStrategy
{
    public PaymentMethodType MethodType => PaymentMethodType.CASH;

    public async Task<bool> ExecuteRefundAsync(Payment payment)
    {
        // COD refund might be manual or simple balance update
        // For now, we just return true to simulate success
        return await Task.FromResult(true);
    }
}
