using AuthModule.Application.Repositories;
using AuthModule.Domain.Entities;

namespace AuthModule.Infrastructure.Repositories;

internal class UserRoleRepository : IUserRoleRepository
{
    public Task AddAsync(UserRole userRole, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}