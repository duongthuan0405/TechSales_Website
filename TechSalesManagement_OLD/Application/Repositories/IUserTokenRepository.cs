using System;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Interfaces;

public interface IUserTokenRepository
{
    Task<Guid> AddAsync(UserToken token);
    Task<UserToken?> GetByUserIdAndTypeAsync(Guid userId, TokenType type);
    Task UpdateAsync(UserToken token);
}
