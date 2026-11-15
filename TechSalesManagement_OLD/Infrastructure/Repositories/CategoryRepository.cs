using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Infrastructure.Persistence;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly TechSalesDbContext _dbContext;

    public CategoryRepository(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Category category)
    {
        var dbModel = new CategoryDbModel
        {
            id = Guid.NewGuid(),
            name = category.name,
            created_at = DateTimeOffset.UtcNow
        };
        await _dbContext.Categories.AddAsync(dbModel);
    }

    public async Task DeleteAsync(Guid id)
    {
        var dbModel = await _dbContext.Categories.FindAsync(id);
        if (dbModel != null)
        {
            _dbContext.Categories.Remove(dbModel);
        }
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        var dbModel = await _dbContext.Categories.FindAsync(id);
        if (dbModel == null) return null;

        return new Category
        {
            id = dbModel.id,
            name = dbModel.name,
            createdAt = dbModel.created_at
        };
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        var dbModel = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.name.ToLower() == name.ToLower());
        
        if (dbModel == null) return null;

        return new Category
        {
            id = dbModel.id,
            name = dbModel.name,
            createdAt = dbModel.created_at
        };
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _dbContext.Categories
            .Select(c => new Category
            {
                id = c.id,
                name = c.name,
                createdAt = c.created_at
            })
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbContext.Categories.AnyAsync(c => c.id == id);
    }
}
