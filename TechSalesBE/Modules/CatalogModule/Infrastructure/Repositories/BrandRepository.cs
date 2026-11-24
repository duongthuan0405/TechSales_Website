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

    public async Task<(List<Brand> Items, int TotalCount)> GetPageAsync(int pageNumber, int pageSize,
        string? keyword, CancellationToken cancellationToken)
    {
        IQueryable<BrandDbModel> query = _db.Brands.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string term = keyword.Trim().ToLowerInvariant();
            query = query.Where(row => row.Name.ToLower().Contains(term));
        }
        int totalCount = await query.CountAsync(cancellationToken);
        List<BrandDbModel> rows = await query.OrderBy(row => row.Name).ThenBy(row => row.Id)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (rows.Select(Map).ToList(), totalCount);
    }

    public async Task<List<Brand>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<BrandDbModel> rows = await _db.Brands.AsNoTracking()
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);
        List<Brand> brands = new List<Brand>();
        foreach (BrandDbModel row in rows)
        {
            brands.Add(Map(row));
        }

        return brands;
    }

    public async Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        BrandDbModel? row = await _db.Brands.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (row == null)
        {
            return null;
        }

        return Map(row);
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
