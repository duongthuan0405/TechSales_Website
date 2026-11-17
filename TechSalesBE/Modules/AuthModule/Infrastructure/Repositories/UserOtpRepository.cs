using AuthModule.Application.Repositories;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using AuthModule.Infrastructure.Persistence;
using AuthModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthModule.Infrastructure.Repositories;
internal sealed class UserOtpRepository : IUserOtpRepository
{
    private readonly AuthDbContext _db;

    public UserOtpRepository(AuthDbContext db)
    {
        _db = db;
    }

    public async Task<UserOtp?> GetByUserIdAndTypeAsync(Guid userId, OtpType type, CancellationToken cancellationToken = default)
    {
        var row = await _db.UserOtps.AsNoTracking().SingleOrDefaultAsync(t => t.UserId == userId && t.Type == type, cancellationToken);
        return row is null ? null : new UserOtp(row.UserId, row.Value, row.Type, row.ExpiredAt)
        {
            Id = row.Id,
            CreatedAt = row.CreatedAt,
            UsedAt = row.UsedAt
        };
    }

    public async Task UpsertAsync(UserOtp token, CancellationToken cancellationToken)
    {
        var row = await _db.UserOtps.SingleOrDefaultAsync(t => t.UserId == token.UserId && t.Type == token.Type, cancellationToken);
        if (row is null)
        {
            row = new UserOtpDbModel
            {
                Id = Guid.NewGuid(),
                UserId = token.UserId,
                Type = token.Type,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.UserOtps.Add(row);
        }

        row.Value = token.Otp;
        row.ExpiredAt = token.ExpiredAt;
        row.UsedAt = token.UsedAt;
    }

    public async Task InvalidateAsync(Guid userId, OtpType type, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var row = await _db.UserOtps.SingleOrDefaultAsync(t => t.UserId == userId && t.Type == type, cancellationToken);
        if (row is not null)
        {
            row.UsedAt = now;
        }
    }
}
