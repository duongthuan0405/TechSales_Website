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

    public Task SaveAsync(Category category, CancellationToken cancellationToken)
    {
        CategoryDbModel row = new CategoryDbModel
        {
            Id = category.Id,
            Name = category.Name
        };

        _db.Categorys.Add(row);
        return Task.CompletedTask;
    }

    private static Category Map(CategoryDbModel row)
    {
        return new Category(row.Id, row.Name);
    }
}
