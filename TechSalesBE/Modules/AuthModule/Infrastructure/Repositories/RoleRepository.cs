using AuthModule.Application.Repositories;
using AuthModule.Domain.Entities;
using AuthModule.Infrastructure.Persistence;
using AuthModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthModule.Infrastructure.Repositories;
internal sealed class RoleRepository : IRoleRepository
{
    private readonly AuthDbContext _db;

    public RoleRepository(AuthDbContext db)
    {
        _db = db;
    }

    public async Task LockAsync(Guid roleId, CancellationToken cancellationToken)
    {
        if (_db.Database.IsNpgsql())
        {
            await _db.Roles.FromSqlInterpolated($"SELECT * FROM identify_access.\"Role\" WHERE \"Id\" = {roleId} FOR UPDATE").AsNoTracking().ToListAsync(cancellationToken);
        }
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default) => await _db.Roles.Where(r => r.Name == name).Select(r => new Role(r.Id, r.Name, r.Description)).SingleOrDefaultAsync(cancellationToken);
    public async Task<List<Role>> GetByIdsAsync(Guid[] ids, CancellationToken cancellationToken) => await _db.Roles.Where(r => ids.Contains(r.Id)).Select(r => new Role(r.Id, r.Name, r.Description)).ToListAsync(cancellationToken);
    public async Task<List<Role>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<RoleDbModel> rows = await _db.Roles.AsNoTracking().Include(role => role.RolePermissions).ThenInclude(rolePermission => rolePermission.Permission).OrderBy(role => role.Name).ToListAsync(cancellationToken);
        List<Role> roles = new List<Role>();
        foreach (RoleDbModel row in rows)
        {
            Role role = new Role(row.Id, row.Name, row.Description);
            foreach (RolePermissionDbModel rolePermission in row.RolePermissions)
            {
                PermissionDbModel permissionRow = rolePermission.Permission;
                Permission permission = new Permission(permissionRow.Id, permissionRow.Code, permissionRow.Name, permissionRow.Module);
                role.Permissions.Add(permission);
            }

            roles.Add(role);
        }

        return roles;
    }
}
