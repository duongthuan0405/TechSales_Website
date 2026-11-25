using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Interfaces;

public interface IPaymentMethodService
{
    Task<List<PaymentMethod>> GetAllPaymentMethodsAsync();
}
