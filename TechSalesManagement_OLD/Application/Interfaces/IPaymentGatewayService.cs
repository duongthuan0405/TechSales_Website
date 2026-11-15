using System;
using System.Threading.Tasks;

namespace TechSalesManagement.Application.Interfaces;

public interface IPaymentGatewayService
{
    Task<bool> RefundAsync(string transactionRef, decimal amount);
}
