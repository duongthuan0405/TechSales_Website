using AuthModule.Domain.Entities;
namespace AuthModule.Application.Repositories;
internal interface IPermissionRepository
{
    Task<bool> CheckUserHasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default);
    Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken);
    Task<bool> AllExistAsync(Guid[] ids, CancellationToken cancellationToken);
    Task ReplaceRolePermissionsAsync(Guid roleId, Guid[] ids, CancellationToken cancellationToken);
}
