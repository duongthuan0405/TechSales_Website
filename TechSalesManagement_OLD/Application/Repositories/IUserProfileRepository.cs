using System;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Interfaces;

public interface IUserProfileRepository
{
    Task AddAsync(UserProfile profile);
    Task UpdateAsync(UserProfile profile);
    Task<UserProfile?> GetByUserIdAsync(Guid userId);
}
