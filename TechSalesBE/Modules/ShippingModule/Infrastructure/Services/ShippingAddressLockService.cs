using Microsoft.EntityFrameworkCore;
using ShippingModule.Application.Services;
using ShippingModule.Infrastructure.Persistence;

namespace ShippingModule.Infrastructure.Services;

internal sealed class ShippingAddressLockService : IShippingAddressLockService
{
    private readonly ShippingDbContext _db;

    public ShippingAddressLockService(ShippingDbContext db)
    {
        _db = db;
    }

    public async Task LockByUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (_db.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException("Address write locks require an active transaction.");
        }

        if (_db.Database.IsNpgsql())
        {
            // Serialize writes for this user even when no addresses exist yet.
            string lockKey = $"shipping-address:{userId:D}";
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))", cancellationToken);
        }
    }
}
