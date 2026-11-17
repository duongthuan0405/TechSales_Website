namespace AuthModule.Application.Repositories;
internal interface IUserRoleRepository
{
    Task ReplaceAsync(Guid userId, Guid[] roleIds, CancellationToken cancellationToken);
}
