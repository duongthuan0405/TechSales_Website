using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Implementations;

public class SystemSettingService : ISystemSettingService
{
    private readonly ISystemSettingRepository _settingRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SystemSettingService(
        ISystemSettingRepository settingRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork)
    {
        _settingRepository = settingRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task UpdateSettingAsync(string key, string value, string? description, Guid staffId)
    {
        var oldValue = await GetValueAsync(key);

        try
        {
            await _unitOfWork.BeginAsync();

            var setting = new SystemSetting(key, value, description);
            await _settingRepository.UpsertAsync(setting);

            var auditLog = new AuditLog(staffId, "UPDATE_SETTING", "SystemSettings", key)
            {
                oldValues = System.Text.Json.JsonSerializer.Serialize(new { value = oldValue }),
                newValues = System.Text.Json.JsonSerializer.Serialize(new { value = value }),
                affectedColumns = "value"
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<List<SystemSetting>> GetAllSettingsAsync()
    {
        return await _settingRepository.GetAllAsync();
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var setting = await _settingRepository.GetByKeyAsync(key);
        return setting?.value;
    }
}
