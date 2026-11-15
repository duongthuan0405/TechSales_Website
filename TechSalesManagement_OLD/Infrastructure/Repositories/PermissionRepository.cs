using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Infrastructure.Persistence;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly TechSalesDbContext _dbContext;

    public PermissionRepository(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Permission>> GetAllAsync()
    {
        var dbModels = await _dbContext.Permissions.ToListAsync();
        return dbModels.Select(m => MapToEntity(m)!).ToList();
    }

    public async Task<List<Permission>> GetByIdsAsync(List<Guid> ids)
    {
        var dbModels = await _dbContext.Permissions
            .Where(p => ids.Contains(p.id))
            .ToListAsync();
        return dbModels.Select(m => MapToEntity(m)!).ToList();
    }

    public async Task<Permission?> GetByCodeAsync(string code)
    {
        var dbModel = await _dbContext.Permissions
            .FirstOrDefaultAsync(p => p.code == code);
        return MapToEntity(dbModel);
    }

    private Permission? MapToEntity(PermissionDbModel? dbModel)
    {
        if (dbModel == null) return null;
        return new Permission
        {
            id = dbModel.id,
            code = dbModel.code,
            name = dbModel.name,
            module = dbModel.module,
            createdAt = dbModel.created_at,
            updatedAt = dbModel.updated_at
        };
    }
}
