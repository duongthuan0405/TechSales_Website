using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<Guid> AddAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> ExistsByEmailAsync(string email);
    Task<List<User>> GetUsersByRoleAsync(string roleName, int pageNumber, int pageSize);
    Task<(List<User> items, int totalCount)> GetPagedUsersByRoleAsync(string roleName, int pageNumber, int pageSize);
    Task<(List<User> items, int totalCount)> GetPagedUsersByRolesAsync(string[] roleNames, int pageNumber, int pageSize);
    Task UpdateStatusAsync(Guid userId, UserStatus status, DateTimeOffset? lockedUntil);
    Task DeleteAsync(Guid id);
}
