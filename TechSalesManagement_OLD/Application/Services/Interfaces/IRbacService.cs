using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Interfaces;

public interface IRbacService
{
    Task<List<Role>> GetRolesAsync();
    Task<List<Permission>> GetPermissionsAsync();
    Task UpdateRolePermissionsAsync(Guid roleId, List<Guid> permissionIds, Guid staffId);
    Task AssignUserRolesAsync(Guid userId, List<Guid> roleIds, Guid staffId);
}
