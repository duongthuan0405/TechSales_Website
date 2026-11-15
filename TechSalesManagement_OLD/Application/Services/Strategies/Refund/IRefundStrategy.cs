using System;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Services.Strategies.Refund;

public interface IRefundStrategy
{
    PaymentMethodType MethodType { get; }
    Task<bool> ExecuteRefundAsync(Payment payment);
}
