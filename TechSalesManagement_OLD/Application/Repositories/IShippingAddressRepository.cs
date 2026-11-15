using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Repositories;

public interface IShippingAddressRepository
{
    Task<ShippingAddress?> GetByIdAsync(Guid id);
    Task<ShippingAddress?> GetDefaultAddressByUserIdAsync(Guid userId);
    Task<List<ShippingAddress>> GetAddressesByUserIdAsync(Guid userId);
    Task AddAsync(ShippingAddress address);
    Task UpdateAsync(ShippingAddress address);
}
