using AuthModule.Domain.Entities;

namespace AuthModule.Application.Repositories;

internal interface IRefreshSessionRepository
{
    Task<RefreshSession?> FindAsync(string tokenHash, CancellationToken ct);
    Task AddAsync(RefreshSession session, CancellationToken ct);
    Task RevokeAsync(Guid id, DateTimeOffset now, CancellationToken ct);
    Task RevokeFamilyAsync(Guid familyId, DateTimeOffset now, CancellationToken ct);
    Task RevokeUserAsync(Guid userId, DateTimeOffset now, CancellationToken ct);
}
