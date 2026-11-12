using AuthModule.Application.Repositories;

namespace AuthModule.Infrastructure.Repositories;

internal class PermissionRepository : IPermissionRepository
{
    public Task<bool> CheckUserHasPermissionAsync(string permission, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}