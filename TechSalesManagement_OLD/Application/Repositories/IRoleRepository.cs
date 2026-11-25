using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name);
    Task<System.Collections.Generic.List<Role>> GetAllWithPermissionsAsync();
    Task<Role?> GetByIdWithPermissionsAsync(Guid id);
    Task UpdatePermissionsAsync(Guid roleId, System.Collections.Generic.List<Guid> permissionIds);
    Task AssignUserRolesAsync(Guid userId, System.Collections.Generic.List<Guid> roleIds);
}
