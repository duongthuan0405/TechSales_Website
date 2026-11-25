using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Interfaces;

public interface IAdminUserService
{
    Task<(List<User> items, int totalCount)> GetCustomersAsync(int pageNumber, int pageSize);
    Task<(List<User> items, int totalCount)> GetStaffAsync(int pageNumber, int pageSize, Guid requesterId);
    Task<User> CreateStaffAsync(User user, string password, Guid requesterId);
    Task<User> UpdateStaffAsync(Guid id, User user);

    Task LockCustomerAsync(Guid userId, DateTimeOffset? until, Guid staffId);
    Task UnlockCustomerAsync(Guid userId, Guid staffId);
}
