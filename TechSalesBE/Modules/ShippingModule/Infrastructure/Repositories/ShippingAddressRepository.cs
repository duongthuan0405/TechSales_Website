using Microsoft.EntityFrameworkCore;
using ShippingModule.Application.Repositories;
using ShippingModule.Domain.Entities;
using ShippingModule.Infrastructure.Persistence;
using ShippingModule.Infrastructure.Persistence.Models;

namespace ShippingModule.Infrastructure.Repositories;

internal sealed class ShippingAddressRepository : IShippingAddressRepository
{
    private readonly ShippingDbContext _db;

    public ShippingAddressRepository(ShippingDbContext db)
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

    public async Task<List<ShippingAddress>> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        List<ShippingAddressDbModel> rows = await _db.ShippingAddresss.AsNoTracking()
            .Include(address => address.Ward).ThenInclude(ward => ward.Province)
            .Where(address => address.UserId == userId && address.DeletedAt == null)
            .OrderByDescending(address => address.IsDefault == true)
            .ThenBy(address => address.CreatedAt).ThenBy(address => address.Id)
            .ToListAsync(cancellationToken);
        List<ShippingAddress> addresses = new List<ShippingAddress>();
        foreach (ShippingAddressDbModel row in rows)
        {
            addresses.Add(MapAddress(row));
        }

        return addresses;
    }

    public async Task<ShippingAddress?> GetActiveAsync(Guid id, CancellationToken cancellationToken)
    {
        ShippingAddressDbModel? row = await _db.ShippingAddresss.AsNoTracking()
            .Include(address => address.Ward).ThenInclude(ward => ward.Province)
            .SingleOrDefaultAsync(address => address.Id == id && address.DeletedAt == null, cancellationToken);
        if (row == null)
        {
            return null;
        }

        return MapAddress(row);
    }

    public async Task SaveAsync(ShippingAddress address, CancellationToken cancellationToken)
    {
        ShippingAddressDbModel? row = await _db.ShippingAddresss
            .SingleOrDefaultAsync(item => item.Id == address.Id, cancellationToken);
        if (row == null)
        {
            row = new ShippingAddressDbModel
            {
                Id = address.Id,
                UserId = address.UserId,
                CreatedAt = address.CreatedAt
            };
            _db.ShippingAddresss.Add(row);
        }

        row.WardCode = address.Ward.Code;
        row.Detail = address.Detail;
        row.IsDefault = address.IsDefault;
        row.UpdateAt = address.UpdatedAt;
        row.DeletedAt = address.DeletedAt;
        // The transaction owner saves all pending changes together.
    }

    private static ShippingAddress MapAddress(ShippingAddressDbModel row)
    {
        return new ShippingAddress(row.Id, row.UserId, LocationRepository.MapWard(row.Ward),
            row.Detail, row.IsDefault == true, row.CreatedAt, row.UpdateAt, row.DeletedAt);
    }
}
