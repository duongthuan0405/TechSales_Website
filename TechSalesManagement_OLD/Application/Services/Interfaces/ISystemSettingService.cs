using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Interfaces;

public interface ISystemSettingService
{
    Task UpdateSettingAsync(string key, string value, string? description, Guid staffId);
    Task<List<SystemSetting>> GetAllSettingsAsync();
    Task<string?> GetValueAsync(string key);
}
