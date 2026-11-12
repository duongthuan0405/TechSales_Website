using AuthModule.Application.Repositories;
using AuthModule.Domain.Entities;

namespace AuthModule.Infrastructure.Repositories;

internal class UserRepository : IUserRepository
{
    public Task<Guid> AddAsync(User newUser, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(User existingUser, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}