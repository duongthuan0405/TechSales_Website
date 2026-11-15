using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Repositories;

public interface ISystemSettingRepository
{
    Task<SystemSetting?> GetByKeyAsync(string key);
    Task<List<SystemSetting>> GetAllAsync();
    Task UpsertAsync(SystemSetting setting);
}
