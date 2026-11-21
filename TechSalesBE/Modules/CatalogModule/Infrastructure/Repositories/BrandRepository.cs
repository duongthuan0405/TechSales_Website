using CatalogModule.Application.Repositories;
using CatalogModule.Domain.Entities;
using CatalogModule.Infrastructure.Persistence;
using CatalogModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogModule.Infrastructure.Repositories;

internal sealed class BrandRepository : IBrandRepository
{
    private readonly CatalogDbContext _db;

    public BrandRepository(CatalogDbContext db)
    {
        _db = db;
    }

    public async Task<Brand?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        BrandDbModel? row = await _db.Brands.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Name == name, cancellationToken);
        if (row == null)
        {
            return null;
        }

        return Map(row);
    }

    public async Task SaveAsync(Brand brand, CancellationToken cancellationToken)
    {
        BrandDbModel? row = await _db.Brands
            .SingleOrDefaultAsync(item => item.Id == brand.Id, cancellationToken);

        if (row == null)
        {
            row = new BrandDbModel { Id = brand.Id };
            _db.Brands.Add(row);
        }

        row.Name = brand.Name;
    }

    private static Brand Map(BrandDbModel row)
    {
        return new Brand(row.Id, row.Name);
    }
}
