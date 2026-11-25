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

public class ArticleRepository : IArticleRepository
{
    private readonly TechSalesDbContext _dbContext;

    public ArticleRepository(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Article article)
    {
        var dbModel = new ArticleDbModel
        {
            id = article.id,
            title = article.title,
            slug = article.slug,
            content = article.content,
            thumbnail_url = article.thumbnailUrl,
            status = article.status,
            author_id = article.authorId,
            created_at = article.createdAt,
            updated_at = article.updatedAt
        };
        await _dbContext.Articles.AddAsync(dbModel);
    }

    public async Task UpdateAsync(Article article)
    {
        var dbModel = await _dbContext.Articles.FindAsync(article.id);
        if (dbModel != null)
        {
            dbModel.title = article.title;
            dbModel.slug = article.slug;
            dbModel.content = article.content;
            dbModel.thumbnail_url = article.thumbnailUrl;
            dbModel.status = article.status;
            dbModel.updated_at = DateTimeOffset.UtcNow;
            _dbContext.Articles.Update(dbModel);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var dbModel = await _dbContext.Articles.FindAsync(id);
        if (dbModel != null)
        {
            _dbContext.Articles.Remove(dbModel);
        }
    }

    public async Task<Article?> GetByIdAsync(Guid id)
    {
        var dbModel = await _dbContext.Articles.FindAsync(id);
        return MapToEntity(dbModel);
    }

    public async Task<Article?> GetBySlugAsync(string slug)
    {
        var dbModel = await _dbContext.Articles.FirstOrDefaultAsync(a => a.slug == slug);
        return MapToEntity(dbModel);
    }

    public async Task<(List<Article> items, int totalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var query = _dbContext.Articles.AsQueryable();
        int totalCount = await query.CountAsync();
        var dbModels = await query
            .OrderByDescending(a => a.created_at)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (dbModels.Select(m => MapToEntity(m)!).ToList(), totalCount);
    }

    private Article? MapToEntity(ArticleDbModel? dbModel)
    {
        if (dbModel == null) return null;
        return new Article
        {
            id = dbModel.id,
            title = dbModel.title,
            slug = dbModel.slug,
            content = dbModel.content,
            thumbnailUrl = dbModel.thumbnail_url,
            status = dbModel.status,
            authorId = dbModel.author_id,
            createdAt = dbModel.created_at,
            updatedAt = dbModel.updated_at
        };
    }
}
