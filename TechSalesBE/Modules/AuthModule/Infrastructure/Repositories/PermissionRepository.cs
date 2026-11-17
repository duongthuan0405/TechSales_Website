using AuthModule.Application.Constants;
using AuthModule.Domain.Entities;
using AuthModule.Application.Repositories;
using AuthModule.Domain.Enums;
using AuthModule.Infrastructure.Persistence;
using AuthModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthModule.Infrastructure.Repositories;
internal sealed class PermissionRepository : IPermissionRepository
{
    private readonly AuthDbContext _db;

    public PermissionRepository(AuthDbContext db)
    {
        _db = db;
    }

    public Task<bool> CheckUserHasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default) => _db.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.User.Status == UserStatus.ACTIVE && (ur.Role.Name == UserRoleName.TECHNICAL_ADMIN || ur.Role.RolePermissions.Any(rp => rp.Permission.Code == permission)), cancellationToken);
    public async Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<PermissionDbModel> rows = await _db.Permissions.AsNoTracking().OrderBy(permission => permission.Module).ThenBy(permission => permission.Code).ToListAsync(cancellationToken);
        List<Permission> permissions = new List<Permission>();
        foreach (PermissionDbModel row in rows)
        {
            Permission permission = new Permission(row.Id, row.Code, row.Name, row.Module);
            permissions.Add(permission);
        }

        return permissions;
    }

    public async Task<bool> AllExistAsync(Guid[] ids, CancellationToken cancellationToken) => await _db.Permissions.CountAsync(p => ids.Contains(p.Id), cancellationToken) == ids.Length;
    public async Task ReplaceRolePermissionsAsync(Guid roleId, Guid[] ids, CancellationToken cancellationToken)
    {
        var current = await _db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync(cancellationToken);
        _db.RolePermissions.RemoveRange(current.Where(rp => !ids.Contains(rp.PermissionId)));
        _db.RolePermissions.AddRange(ids.Except(current.Select(rp => rp.PermissionId)).Select(id => new RolePermissionDbModel { RoleId = roleId, PermissionId = id }));
    }
}
