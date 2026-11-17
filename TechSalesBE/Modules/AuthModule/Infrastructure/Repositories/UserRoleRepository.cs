using AuthModule.Application.Repositories;
using AuthModule.Infrastructure.Persistence;
using AuthModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthModule.Infrastructure.Repositories;
internal sealed class UserRoleRepository : IUserRoleRepository
{
    private readonly AuthDbContext _db;

    public UserRoleRepository(AuthDbContext db)
    {
        _db = db;
    }

    public async Task ReplaceAsync(Guid userId, Guid[] roleIds, CancellationToken cancellationToken)
    {
        var current = await _db.UserRoles.Where(r => r.UserId == userId).ToListAsync(cancellationToken);
        _db.UserRoles.RemoveRange(current.Where(r => !roleIds.Contains(r.RoleId)));
        _db.UserRoles.AddRange(roleIds.Except(current.Select(r => r.RoleId)).Select(id => new UserRoleDbModel { UserId = userId, RoleId = id }));
    }
}
