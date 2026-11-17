using AuthModule.Domain.Entities;
namespace AuthModule.Application.Repositories;
internal interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<Role>> GetByIdsAsync(Guid[] ids, CancellationToken cancellationToken);
    Task<List<Role>> GetAllAsync(CancellationToken cancellationToken);
    Task LockAsync(Guid roleId, CancellationToken cancellationToken);
}
