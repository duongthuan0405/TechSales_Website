namespace AuthModule.Application.Repositories;

internal interface IPermissionRepository
{
    Task<bool> CheckUserHasPermissionAsync(string permission, CancellationToken cancellationToken = default);
}