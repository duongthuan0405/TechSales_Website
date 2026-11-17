using Microsoft.EntityFrameworkCore;
using ShippingModule.Application.Repositories;
using ShippingModule.Domain.Entities;
using ShippingModule.Infrastructure.Persistence;
using ShippingModule.Infrastructure.Persistence.Models;

namespace ShippingModule.Infrastructure.Repositories;

internal sealed class LocationRepository : ILocationRepository
{
    private readonly ShippingDbContext _db;

    public LocationRepository(ShippingDbContext db)
    {
        _db = db;
    }

    public async Task<List<Province>> GetProvincesAsync(CancellationToken cancellationToken)
    {
        List<ProvinceDbModel> rows = await _db.Provices.AsNoTracking()
            .OrderBy(province => province.Code).ToListAsync(cancellationToken);
        List<Province> provinces = new List<Province>();
        foreach (ProvinceDbModel row in rows)
        {
            provinces.Add(MapProvince(row));
        }

        return provinces;
    }

    public async Task<Province?> GetProvinceAsync(string code, CancellationToken cancellationToken)
    {
        ProvinceDbModel? row = await _db.Provices.AsNoTracking()
            .SingleOrDefaultAsync(province => province.Code == code, cancellationToken);
        if (row == null)
        {
            return null;
        }

        return MapProvince(row);
    }

    public async Task<List<Ward>> GetWardsAsync(string provinceCode, CancellationToken cancellationToken)
    {
        List<WardDbModel> rows = await _db.Wards.AsNoTracking().Include(ward => ward.Province)
            .Where(ward => ward.ProvinceCode == provinceCode)
            .OrderBy(ward => ward.Code).ToListAsync(cancellationToken);
        List<Ward> wards = new List<Ward>();
        foreach (WardDbModel row in rows)
        {
            wards.Add(MapWard(row));
        }

        return wards;
    }

    public async Task<Ward?> GetWardAsync(string code, CancellationToken cancellationToken)
    {
        WardDbModel? row = await _db.Wards.AsNoTracking().Include(ward => ward.Province)
            .SingleOrDefaultAsync(ward => ward.Code == code, cancellationToken);
        if (row == null)
        {
            return null;
        }

        return MapWard(row);
    }

    internal static Province MapProvince(ProvinceDbModel row)
    {
        return new Province(row.Code.TrimEnd(), row.Name);
    }

    internal static Ward MapWard(WardDbModel row)
    {
        return new Ward(row.Code.TrimEnd(), row.Name, MapProvince(row.Province));
    }
}
