using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Repositories;

public interface IPaymentMethodRepository
{
    Task<List<PaymentMethod>> GetAllAsync();
    Task<PaymentMethod?> GetByIdAsync(Guid id);
    Task AddAsync(PaymentMethod paymentMethod);
    Task<bool> AnyAsync();
}
