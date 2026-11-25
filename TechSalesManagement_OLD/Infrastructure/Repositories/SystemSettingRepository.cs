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

public class SystemSettingRepository : ISystemSettingRepository
{
    private readonly TechSalesDbContext _dbContext;

    public SystemSettingRepository(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SystemSetting?> GetByKeyAsync(string key)
    {
        var dbModel = await _dbContext.SystemSettings.FindAsync(key);
        return MapToEntity(dbModel);
    }

    public async Task<List<SystemSetting>> GetAllAsync()
    {
        var dbModels = await _dbContext.SystemSettings.ToListAsync();
        return dbModels.Select(m => MapToEntity(m)!).ToList();
    }

    public async Task UpsertAsync(SystemSetting setting)
    {
        var dbModel = await _dbContext.SystemSettings.FindAsync(setting.key);
        if (dbModel == null)
        {
            dbModel = new SystemSettingDbModel
            {
                key = setting.key,
                value = setting.value,
                description = setting.description,
                updated_at = DateTimeOffset.UtcNow
            };
            await _dbContext.SystemSettings.AddAsync(dbModel);
        }
        else
        {
            dbModel.value = setting.value;
            dbModel.description = setting.description;
            dbModel.updated_at = DateTimeOffset.UtcNow;
            _dbContext.SystemSettings.Update(dbModel);
        }
    }

    private SystemSetting? MapToEntity(SystemSettingDbModel? dbModel)
    {
        if (dbModel == null) return null;
        return new SystemSetting
        {
            key = dbModel.key,
            value = dbModel.value,
            description = dbModel.description,
            updatedAt = dbModel.updated_at
        };
    }
}
