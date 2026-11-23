using CatalogModule.Application.Repositories;
using CatalogModule.Domain.Entities;
using CatalogModule.Infrastructure.Persistence;
using CatalogModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogModule.Infrastructure.Repositories;

internal sealed class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _db;

    public CategoryRepository(CatalogDbContext db)
    {
        _db = db;
    }

    public async Task<(List<Category> Items, int TotalCount)> GetPageAsync(int pageNumber, int pageSize,
        string? keyword, CancellationToken cancellationToken)
    {
        IQueryable<CategoryDbModel> query = _db.Categorys.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string term = keyword.Trim().ToLowerInvariant();
            query = query.Where(row => row.Name.ToLower().Contains(term));
        }
        int totalCount = await query.CountAsync(cancellationToken);
        List<CategoryDbModel> rows = await query.OrderBy(row => row.Name).ThenBy(row => row.Id)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (rows.Select(Map).ToList(), totalCount);
    }

    public async Task<List<Category>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<CategoryDbModel> rows = await _db.Categorys
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);

        List<Category> categories = new List<Category>();
        foreach (CategoryDbModel row in rows)
        {
            categories.Add(Map(row));
        }

        return categories;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        CategoryDbModel? row = await _db.Categorys
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (row == null)
        {
            return null;
        }

        return Map(row);
    }

    public async Task SaveAsync(Category category, CancellationToken cancellationToken)
    {
        CategoryDbModel? row = await _db.Categorys
            .SingleOrDefaultAsync(item => item.Id == category.Id, cancellationToken);

        if (row == null)
        {
            row = new CategoryDbModel
            {
                Id = category.Id
            };

            _db.Categorys.Add(row);
        }

        row.Name = category.Name;
    }

    private static Category Map(CategoryDbModel row)
    {
        return new Category(row.Id, row.Name);
    }
}
