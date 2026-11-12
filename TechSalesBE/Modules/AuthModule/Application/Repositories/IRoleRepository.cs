using AuthModule.Domain.Entities;

namespace AuthModule.Application.Repositories;

internal interface IRoleRepository
{
    Task<Role> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}