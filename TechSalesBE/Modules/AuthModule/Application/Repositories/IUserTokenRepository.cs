using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;

namespace AuthModule.Application.Repositories;

internal interface IUserTokenRepository
{
    Task AddAsync(UserToken userToken);
    Task<UserToken> GetByUserIdAndTypeAsync(Guid userId, TokenType type);
    Task UpdateAsync(UserToken existingToken);
}