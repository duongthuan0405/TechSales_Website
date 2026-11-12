using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthModule.Domain.Entities;

namespace AuthModule.Application.Repositories
{
    internal interface IUserRepository
    {
        Task<Guid> AddAsync(User newUser, CancellationToken cancellationToken = default);
        Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task UpdateAsync(User existingUser, CancellationToken cancellationToken = default);
    }
}