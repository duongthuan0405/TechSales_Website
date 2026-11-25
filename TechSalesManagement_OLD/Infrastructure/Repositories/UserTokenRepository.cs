using System;
using System.Threading.Tasks;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Infrastructure.Persistence;
using TechSalesManagement.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Infrastructure.Repositories;

public class UserTokenRepository : IUserTokenRepository
{
    private readonly TechSalesDbContext _dbContext;

    public UserTokenRepository(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> AddAsync(UserToken token)
    {
        token.id = Guid.NewGuid();
        token.createdAt = DateTimeOffset.UtcNow;

        var dbModel = MapToDbModel(token);
        await _dbContext.UserTokens.AddAsync(dbModel);
        return token.id;
    }

    public async Task<UserToken?> GetByUserIdAndTypeAsync(Guid userId, TokenType type)
    {
        var dbModel = await _dbContext.UserTokens
            .FirstOrDefaultAsync(x => x.user_id == userId && x.type == type);
        
        return dbModel == null ? null : MapToDomainModel(dbModel);
    }

    public async Task UpdateAsync(UserToken token)
    {
        var dbModel = await _dbContext.UserTokens
            .FirstOrDefaultAsync(x => x.id == token.id);

        if (dbModel != null)
        {
            dbModel.token = token.token;
            dbModel.expired_at = token.expiredAt;
            dbModel.used_at = token.usedAt;
            // Update other relevant fields if needed
            _dbContext.UserTokens.Update(dbModel);
        }
    }

    private UserTokenDbModel MapToDbModel(UserToken token)
    {
        return new UserTokenDbModel
        {
            id = token.id,
            user_id = token.userId,
            token = token.token,
            type = token.type,
            expired_at = token.expiredAt,
            used_at = token.usedAt,
            created_at = token.createdAt
        };
    }
    private UserToken MapToDomainModel(UserTokenDbModel dbModel)
    {
        return new UserToken
        {
            id = dbModel.id,
            userId = dbModel.user_id,
            token = dbModel.token,
            type = dbModel.type,
            expiredAt = dbModel.expired_at,
            usedAt = dbModel.used_at,
            createdAt = dbModel.created_at
        };
    }
}
