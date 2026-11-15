using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;
using TechSalesManagement.Infrastructure.Persistence;

namespace TechSalesManagement.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly TechSalesDbContext _context;

    public PaymentRepository(TechSalesDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetPaymentByOrderIdAsync(Guid orderId)
    {
        var r = await _context.Payments
            .Where(p => p.order_id == orderId)
            .OrderByDescending(p => p.created_at)
            .FirstOrDefaultAsync();

        if(r == null)
        {
            return null;
        }

        return new Payment
        {
            id = r.id,
            orderId = r.order_id,
            paymentMethodId = r.payment_method_id,
            amount = r.amount,
            status = r.status,
            transactionRef = r.transaction_ref,
            createdAt = r.created_at,
            updatedAt = r.updated_at
        };
    }

    public async Task UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status, string? transactionRef = null)
    {
        var payment = await _context.Payments.FindAsync(paymentId);
        if (payment != null)
        {
            payment.status = status;
            if (transactionRef != null)
            {
                payment.transaction_ref = transactionRef;
            }
            _context.Payments.Update(payment);
        }
        Console.WriteLine($"Payment {paymentId} updated to status {status} with transaction ref {transactionRef}");
    }

    public async Task AddPaymentAsync(Payment payment)
    {
        await _context.Payments.AddAsync(new Persistence.Models.PaymentDbModel
        {
            id = payment.id,
            order_id = payment.orderId,
            payment_method_id = payment.paymentMethodId,
            amount = payment.amount,
            status = payment.status,
            transaction_ref = payment.transactionRef,
            created_at = payment.createdAt,
            updated_at = payment.updatedAt ?? DateTimeOffset.UtcNow
        });
    }
}
