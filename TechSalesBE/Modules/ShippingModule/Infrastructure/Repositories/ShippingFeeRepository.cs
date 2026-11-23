using Microsoft.EntityFrameworkCore;
using ShippingModule.Application.Repositories;
using ShippingModule.Domain.Entities;
using ShippingModule.Infrastructure.Persistence;
using ShippingModule.Infrastructure.Persistence.Models;

namespace ShippingModule.Infrastructure.Repositories;

internal sealed class ShippingFeeRepository : IShippingFeeRepository
{
    private readonly ShippingDbContext _db;

    public ShippingFeeRepository(ShippingDbContext db)
    {
        _db = db;
    }

    public async Task SaveAsync(ShippingFee shippingFee, CancellationToken cancellationToken)
    {
        if (_db.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException("Saving shipping fees requires an active transaction.");
        }

        ShippingFeeDbModel? row = await _db.ShippingFees
            .SingleOrDefaultAsync(item => item.ProviceCode == shippingFee.ProvinceCode, cancellationToken);
        if (row == null)
        {
            row = new ShippingFeeDbModel
            {
                ProviceCode = shippingFee.ProvinceCode,
                Fee = shippingFee.Fee
            };
            _db.ShippingFees.Add(row);
        }
        else
        {
            row.Fee = shippingFee.Fee;
        }
    }

    public async Task<ShippingFee?> GetByProvinceAsync(string provinceCode, CancellationToken cancellationToken)
    {
        ShippingFeeDbModel? row = await _db.ShippingFees.AsNoTracking()
            .SingleOrDefaultAsync(item => item.ProviceCode == provinceCode, cancellationToken);
        if (row == null)
        {
            return null;
        }

        return new ShippingFee(row.ProviceCode.TrimEnd(), row.Fee);
    }
}
