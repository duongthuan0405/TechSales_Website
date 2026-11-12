using AuthModule.Application.Repositories;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;

namespace AuthModule.Infrastructure.Repositories;

internal class UserTokenRepository : IUserTokenRepository
{
    public Task AddAsync(UserToken userToken)
    {
        throw new NotImplementedException();
    }

    public Task<UserToken> GetByUserIdAndTypeAsync(Guid userId, TokenType type)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(UserToken existingToken)
    {
        throw new NotImplementedException();
    }
}