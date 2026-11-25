using System;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetPaymentByOrderIdAsync(Guid orderId);
    Task UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status, string? transactionRef = null);
    Task AddPaymentAsync(Payment payment);
}
