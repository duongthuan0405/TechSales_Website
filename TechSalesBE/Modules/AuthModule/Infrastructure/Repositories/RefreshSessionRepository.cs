using AuthModule.Application.Repositories;
using AuthModule.Domain.Entities;
using AuthModule.Infrastructure.Persistence;
using AuthModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthModule.Infrastructure.Repositories;

internal sealed class RefreshSessionRepository(AuthDbContext db) : IRefreshSessionRepository
{
    public async Task<RefreshSession?> FindAsync(string tokenHash, CancellationToken ct)
    {
        var row = await db.RefreshSessions.AsNoTracking().SingleOrDefaultAsync(s => s.TokenHash == tokenHash, ct);
        return row == null ? null : new RefreshSession { Id = row.Id, UserId = row.UserId, FamilyId = row.FamilyId,
            TokenHash = row.TokenHash, CreatedAt = row.CreatedAt, ExpiresAt = row.ExpiresAt, RevokedAt = row.RevokedAt };
    }

    public async Task AddAsync(RefreshSession session, CancellationToken ct) => await db.RefreshSessions.AddAsync(
        new RefreshSessionDbModel { Id = session.Id, UserId = session.UserId, FamilyId = session.FamilyId,
            TokenHash = session.TokenHash, CreatedAt = session.CreatedAt, ExpiresAt = session.ExpiresAt }, ct);

    public async Task RevokeAsync(Guid id, DateTimeOffset now, CancellationToken ct)
    {
        var row = await db.RefreshSessions.SingleAsync(s => s.Id == id, ct);
        row.RevokedAt ??= now;
    }

    public async Task RevokeFamilyAsync(Guid familyId, DateTimeOffset now, CancellationToken ct)
    {
        var rows = await db.RefreshSessions.Where(s => s.FamilyId == familyId && s.RevokedAt == null).ToListAsync(ct);
        foreach (var row in rows) row.RevokedAt = now;
    }

    public async Task RevokeUserAsync(Guid userId, DateTimeOffset now, CancellationToken ct)
    {
        var rows = await db.RefreshSessions.Where(s => s.UserId == userId && s.RevokedAt == null).ToListAsync(ct);
        foreach (var row in rows) row.RevokedAt = now;
    }
}
