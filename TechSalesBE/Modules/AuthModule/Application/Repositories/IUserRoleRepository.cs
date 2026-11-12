using AuthModule.Domain.Entities;

namespace AuthModule.Application.Repositories;

internal interface IUserRoleRepository
{
    Task AddAsync(UserRole userRole, CancellationToken cancellationToken = default);
}