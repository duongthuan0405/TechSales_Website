using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Repositories;

public interface IPermissionRepository
{
    Task<List<Permission>> GetAllAsync();
    Task<List<Permission>> GetByIdsAsync(List<Guid> ids);
    Task<Permission?> GetByCodeAsync(string code);
}
