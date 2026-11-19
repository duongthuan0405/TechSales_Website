using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Repositories;

public interface IPaymentMethodRepository
{
    Task<List<PaymentMethod>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PaymentMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
}
