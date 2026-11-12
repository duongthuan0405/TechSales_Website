using AuthModule.Application.Repositories;
using AuthModule.Domain.Entities;

namespace AuthModule.Infrastructure.Repositories;

internal class RoleRepository : IRoleRepository
{
    public Task<Role> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}