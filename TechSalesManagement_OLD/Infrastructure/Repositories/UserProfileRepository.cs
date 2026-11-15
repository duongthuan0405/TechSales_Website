using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Infrastructure.Persistence;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly TechSalesDbContext _dbContext;

    public UserProfileRepository(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserProfile?> GetByUserIdAsync(Guid userId)
    {
        var dbModel = await _dbContext.UserProfiles
            .FirstOrDefaultAsync(up => up.user_id == userId);

        if (dbModel == null) return null;

        return new UserProfile
        {
            id = dbModel.user_id,
            userId = dbModel.user_id,
            fullName = dbModel.full_name,
            phone = dbModel.phone,
            avatarUrl = dbModel.avatar_url,
            dateOfBirth = dbModel.date_of_birth,
            createdAt = dbModel.created_at,
            updatedAt = dbModel.updated_at
        };
    }

    public async Task AddAsync(UserProfile profile)
    {
        var dbModel = new UserProfileDbModel
        {
            user_id = profile.userId,
            full_name = profile.fullName,
            phone = profile.phone,
            avatar_url = profile.avatarUrl,
            date_of_birth = profile.dateOfBirth,
            created_at = DateTimeOffset.UtcNow
        };

        await _dbContext.UserProfiles.AddAsync(dbModel);
    }

    public async Task UpdateAsync(UserProfile profile)
    {
        var dbModel = await _dbContext.UserProfiles
            .FirstOrDefaultAsync(up => up.user_id == profile.userId);

        if (dbModel != null)
        {
            dbModel.full_name = profile.fullName;
            dbModel.phone = profile.phone;
            dbModel.avatar_url = profile.avatarUrl;
            dbModel.date_of_birth = profile.dateOfBirth;
            dbModel.updated_at = DateTimeOffset.UtcNow;

            _dbContext.UserProfiles.Update(dbModel);
        }
    }
}
