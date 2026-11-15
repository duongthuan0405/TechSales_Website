using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Infrastructure.Persistence;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly TechSalesDbContext _dbContext;

    public RoleRepository(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        var dbModel = await _dbContext.Roles
            .Include(r => r.role_permissions)
                .ThenInclude(rp => rp.permission)
            .FirstOrDefaultAsync(r => r.name == name);

        return MapToEntity(dbModel);
    }

    public async Task<System.Collections.Generic.List<Role>> GetAllWithPermissionsAsync()
    {
        var dbModels = await _dbContext.Roles
            .Include(r => r.role_permissions)
                .ThenInclude(rp => rp.permission)
            .OrderBy(r => r.name)
            .ToListAsync();

        return dbModels.Select(m => MapToEntity(m)!).ToList();
    }

    public async Task<Role?> GetByIdWithPermissionsAsync(Guid id)
    {
        var dbModel = await _dbContext.Roles
            .Include(r => r.role_permissions)
                .ThenInclude(rp => rp.permission)
            .FirstOrDefaultAsync(r => r.id == id);

        return MapToEntity(dbModel);
    }

    public async Task UpdatePermissionsAsync(Guid roleId, System.Collections.Generic.List<Guid> permissionIds)
    {
        // Sync Strategy: Remove existing, add new
        var existing = await _dbContext.RolePermissions
            .Where(rp => rp.role_id == roleId)
            .ToListAsync();

        _dbContext.RolePermissions.RemoveRange(existing);

        var newPermissions = permissionIds.Select(pId => new RolePermissionDbModel
        {
            role_id = roleId,
            permission_id = pId
        });

        await _dbContext.RolePermissions.AddRangeAsync(newPermissions);
    }

    public async Task AssignUserRolesAsync(Guid userId, System.Collections.Generic.List<Guid> roleIds)
    {
        var existing = await _dbContext.UserRoles
            .Where(ur => ur.user_id == userId)
            .ToListAsync();

        _dbContext.UserRoles.RemoveRange(existing);

        var newUserRoles = roleIds.Select(rId => new UserRoleDbModel
        {
            user_id = userId,
            role_id = rId
        });

        await _dbContext.UserRoles.AddRangeAsync(newUserRoles);
    }

    private Role? MapToEntity(RoleDbModel? dbModel)
    {
        if (dbModel == null) return null;
        var role = new Role
        {
            id = dbModel.id,
            name = dbModel.name,
            description = dbModel.description,
            createdAt = dbModel.created_at,
            updatedAt = null
        };

        if (dbModel.role_permissions != null)
        {
            role.permissions = dbModel.role_permissions
                .Where(rp => rp.permission != null)
                .Select(rp => new Permission
                {
                    id = rp.permission.id,
                    code = rp.permission.code,
                    name = rp.permission.name,
                    module = rp.permission.module
                }).ToList();
        }

        return role;
    }
}
