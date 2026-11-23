using AuthModule.Domain.Entities;
namespace AuthModule.Application.Repositories;
internal interface IUserRepository
{
    Task<Guid> AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default, bool forUpdate = false);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task LockUsersAsync(Guid[] ids, CancellationToken cancellationToken);
    Task<(List<User> Items, int Count)> GetPageAsync(List<string> roles, int page, int size, CancellationToken cancellationToken);
}
