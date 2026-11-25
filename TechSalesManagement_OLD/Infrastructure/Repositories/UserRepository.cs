using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Infrastructure.Persistence;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TechSalesDbContext _dbContext;

    public UserRepository(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        var dbModel = await _dbContext.Users
            .Include(u => u.user_roles)
                .ThenInclude(ur => ur.role)
            .Include(u => u.user_profile)
            .FirstOrDefaultAsync(u => u.id == id);
            
        return MapToEntity(dbModel);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var dbModel = await _dbContext.Users
            .Include(u => u.user_roles)
                .ThenInclude(ur => ur.role)
            .Include(u => u.user_profile)
            .FirstOrDefaultAsync(u => u.email == email);
            
        return MapToEntity(dbModel);
    }

    public async Task<Guid> AddAsync(User user)
    {
        user.id = Guid.NewGuid();
        user.createdAt = DateTimeOffset.UtcNow;

        var dbModel = MapToDbModel(user);
        await _dbContext.Users.AddAsync(dbModel);

        // Lưu các bản ghi quan hệ Many-to-Many (UserRole) một cách tường minh ở tầng Repository
        if (user.roles != null && user.roles.Any())
        {
            foreach (var role in user.roles)
            {
                var userRole = new UserRoleDbModel
                {
                    user_id = user.id,
                    role_id = role.id
                };
                await _dbContext.UserRoles.AddAsync(userRole);
            }
        }

        return user.id;
    }

    public async Task UpdateAsync(User user)
    {
        var dbModel = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.id == user.id);
            
        if (dbModel != null)
        {
            dbModel.email = user.email;
            dbModel.password = user.password;
            dbModel.status = user.status;
            dbModel.failed_login_attempts = user.failedLoginAttempts;
            dbModel.last_failed_at = user.lastFailedAt;
            dbModel.locked_until = user.lockedUntil;
            dbModel.updated_at = DateTimeOffset.UtcNow;
            
            _dbContext.Users.Update(dbModel);
        }
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.email == email);
    }

    public async Task<System.Collections.Generic.List<User>> GetUsersByRoleAsync(string roleName, int pageNumber, int pageSize)
    {
        var dbModels = await _dbContext.Users
            .Include(u => u.user_roles)
                .ThenInclude(ur => ur.role)
            .Include(u => u.user_profile)
            .Where(u => u.user_roles.Any(ur => ur.role.name == roleName))
            .OrderByDescending(u => u.created_at)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return dbModels.Select(m => MapToEntity(m)!).ToList();
    }

    public async Task<(System.Collections.Generic.List<User> items, int totalCount)> GetPagedUsersByRoleAsync(string roleName, int pageNumber, int pageSize)
    {
        var query = _dbContext.Users
            .Include(u => u.user_roles)
                .ThenInclude(ur => ur.role)
            .Include(u => u.user_profile)
            .Where(u => u.user_roles.Any(ur => ur.role.name == roleName));

        int totalCount = await query.CountAsync();

        var dbModels = await query
            .OrderByDescending(u => u.created_at)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = dbModels.Select(m => MapToEntity(m)!).ToList();

        return (items, totalCount);
    }

    public async Task<(System.Collections.Generic.List<User> items, int totalCount)> GetPagedUsersByRolesAsync(string[] roleNames, int pageNumber, int pageSize)
    {
        var query = _dbContext.Users
            .Include(u => u.user_roles)
                .ThenInclude(ur => ur.role)
            .Include(u => u.user_profile)
            .Where(u => u.user_roles.Any(ur => roleNames.Contains(ur.role.name)));

        int totalCount = await query.CountAsync();

        var dbModels = await query
            .OrderByDescending(u => u.created_at)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = dbModels.Select(m => MapToEntity(m)!).ToList();

        return (items, totalCount);
    }

    public async Task UpdateStatusAsync(Guid userId, TechSalesManagement.Domain.Enums.UserStatus status, DateTimeOffset? lockedUntil)
    {
        var dbModel = await _dbContext.Users.FindAsync(userId);
        if (dbModel != null)
        {
            dbModel.status = status;
            dbModel.locked_until = lockedUntil;
            dbModel.updated_at = DateTimeOffset.UtcNow;
            _dbContext.Users.Update(dbModel);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var dbModel = await _dbContext.Users.FindAsync(id);
        if (dbModel != null)
        {
            _dbContext.Users.Remove(dbModel);
        }
    }

    private User? MapToEntity(UserDbModel? dbModel)
    {
        if (dbModel == null) return null;
        
        var user = new User
        {
            id = dbModel.id,
            email = dbModel.email,
            password = dbModel.password,
            status = dbModel.status,
            failedLoginAttempts = dbModel.failed_login_attempts,
            lastFailedAt = dbModel.last_failed_at,
            lockedUntil = dbModel.locked_until,
            createdAt = dbModel.created_at,
            updatedAt = dbModel.updated_at
        };

        // Map roles collection if loaded via Include
        if (dbModel.user_roles != null && dbModel.user_roles.Any())
        {
            user.roles = dbModel.user_roles
                .Where(ur => ur.role != null)
                .Select(ur => new Role
                {
                    id = ur.role.id,
                    name = ur.role.name,
                    description = ur.role.description
                }).ToList();
        }

        // Map profile if loaded via Include
        if (dbModel.user_profile != null)
        {
            user.profile = new UserProfile
            {
                userId = dbModel.user_profile.user_id,
                fullName = dbModel.user_profile.full_name,
                phone = dbModel.user_profile.phone,
                avatarUrl = dbModel.user_profile.avatar_url,
                dateOfBirth = dbModel.user_profile.date_of_birth,
                createdAt = dbModel.user_profile.created_at,
                updatedAt = dbModel.user_profile.updated_at
            };
        }

        return user;
    }

    private UserDbModel MapToDbModel(User user)
    {
        return new UserDbModel
        {
            id = user.id,
            email = user.email,
            password = user.password,
            status = user.status,
            failed_login_attempts = user.failedLoginAttempts,
            last_failed_at = user.lastFailedAt,
            locked_until = user.lockedUntil,
            created_at = user.createdAt,
            updated_at = user.updatedAt
        };
    }
}
