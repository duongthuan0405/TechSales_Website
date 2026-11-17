using AuthModule.Application.Repositories;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Rules;
using AuthModule.Infrastructure.Persistence;
using AuthModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthModule.Infrastructure.Repositories;
internal sealed class UserRepository : IUserRepository
{
    private readonly AuthDbContext _db;

    public UserRepository(AuthDbContext db)
    {
        _db = db;
    }

    private static IQueryable<UserDbModel> WithDetails(IQueryable<UserDbModel> query) => query.AsNoTracking().Include(u => u.Profile).Include(u => u.UserRoles).ThenInclude(ur => ur.Role).AsSplitQuery();
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default, bool forUpdate = false)
    {
        email = AuthRules.NormalizeEmail(email);
        IQueryable<UserDbModel> query = _db.Users;
        if (forUpdate && _db.Database.IsNpgsql())
        {
            query = _db.Users.FromSqlInterpolated($"SELECT * FROM identify_access.\"User\" WHERE lower(\"Email\") = {email} FOR UPDATE");
        }

        var row = await WithDetails(query).SingleOrDefaultAsync(u => u.Email.ToLower() == email, cancellationToken);
        return row is null ? null : Map(row);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await WithDetails(_db.Users).SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
        return row is null ? null : Map(row);
    }

    public async Task LockUsersAsync(Guid[] ids, CancellationToken cancellationToken)
    {
        if (_db.Database.IsNpgsql())
        {
            await _db.Users.FromSqlInterpolated($"SELECT * FROM identify_access.\"User\" WHERE \"Id\" = ANY({ids}) ORDER BY \"Id\" FOR UPDATE").AsNoTracking().ToListAsync(cancellationToken);
        }
    }

    public async Task<Guid> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _db.Users.AddAsync(new UserDbModel { Id = user.Id, Email = user.Email, HashedPassword = user.Password, Status = user.Status, CreatedAt = user.CreatedAt, UpdatedAt = user.UpdatedAt, FailedLoginAttempts = user.FailedLoginAttempts, LastLoginFailedAt = user.LastLoginFailedAt, LockedUntil = user.LockedUntil, Profile = new UserProfileDbModel { UserId = user.Id, FullName = user.Profile.FullName, PhoneNumber = user.Profile.PhoneNumber, CreatedAt = user.CreatedAt }, UserRoles = user.Roles.Select(r => new UserRoleDbModel { UserId = user.Id, RoleId = r.Id }).ToList() });
        return user.Id;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        UserDbModel row = await _db.Users.Include(u => u.Profile).SingleAsync(u => u.Id == user.Id, cancellationToken);
        row.Email = user.Email;
        row.HashedPassword = user.Password;
        row.Status = user.Status;
        row.FailedLoginAttempts = user.FailedLoginAttempts;
        row.LastLoginFailedAt = user.LastLoginFailedAt;
        row.LockedUntil = user.LockedUntil;
        row.UpdatedAt = user.UpdatedAt;
        if (row.Profile == null)
        {
            row.Profile = new UserProfileDbModel
            {
                UserId = user.Id,
                CreatedAt = user.CreatedAt
            };
        }

        row.Profile.FullName = user.Profile.FullName;
        row.Profile.PhoneNumber = user.Profile.PhoneNumber;
        row.Profile.AvatarUrl = user.Profile.AvatarUrl;
        row.Profile.DateOfBirth = user.Profile.DateOfBirth;
        row.Profile.UpdatedAt = user.UpdatedAt;
    }

    public async Task<(List<User> Items, int Count)> GetPageAsync(string[] roles, int page, int size, CancellationToken cancellationToken)
    {
        var query = _db.Users.AsNoTracking().Where(u => u.UserRoles.Any(ur => roles.Contains(ur.Role.Name)));
        var count = await query.CountAsync(cancellationToken);
        var rows = await WithDetails(query.OrderBy(u => u.CreatedAt).ThenBy(u => u.Id).Skip((page - 1) * size).Take(size)).ToListAsync(cancellationToken);
        return (rows.Select(Map).ToList(), count);
    }

    private static User Map(UserDbModel row)
    {
        User user = new User
        {
            Id = row.Id,
            Email = row.Email,
            Password = row.HashedPassword,
            Status = row.Status,
            FailedLoginAttempts = row.FailedLoginAttempts,
            LastLoginFailedAt = row.LastLoginFailedAt,
            LockedUntil = row.LockedUntil,
            CreatedAt = row.CreatedAt,
            UpdatedAt = row.UpdatedAt
        };
        foreach (UserRoleDbModel userRole in row.UserRoles)
        {
            Role role = new Role(userRole.Role.Id, userRole.Role.Name, userRole.Role.Description);
            user.Roles.Add(role);
        }

        if (row.Profile != null)
        {
            user.Profile = new UserProfile
            {
                FullName = row.Profile.FullName,
                PhoneNumber = row.Profile.PhoneNumber,
                AvatarUrl = row.Profile.AvatarUrl,
                DateOfBirth = row.Profile.DateOfBirth
            };
        }

        return user;
    }
}
