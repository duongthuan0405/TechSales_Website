using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TechSalesManagement.Application.Exceptions;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Services.Implementations;

public class ContentManagementService : IContentManagementService
{
    private readonly IArticleRepository _articleRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ContentManagementService(
        IArticleRepository articleRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork)
    {
        _articleRepository = articleRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Article> CreateArticleAsync(string title, string content, string? thumbnailUrl, Guid authorId)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new BadRequestException("Title is required.");

        var slug = GenerateSlug(title);
        var existing = await _articleRepository.GetBySlugAsync(slug);
        if (existing != null) slug = $"{slug}-{DateTime.UtcNow.Ticks}";

        try
        {
            await _unitOfWork.BeginAsync();

            var article = new Article(title, content, authorId)
            {
                id = Guid.NewGuid(),
                slug = slug,
                thumbnailUrl = thumbnailUrl,
                status = ArticleStatus.DRAFT
            };

            await _articleRepository.AddAsync(article);

            var auditLog = new AuditLog(authorId, "CREATE_ARTICLE", "Articles", article.id.ToString())
            {
                newValues = System.Text.Json.JsonSerializer.Serialize(new { title = article.title })
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();
            return article;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateArticleAsync(Guid id, string title, string content, string? thumbnailUrl, Guid staffId)
    {
        var article = await _articleRepository.GetByIdAsync(id);
        if (article == null) throw new NotFoundException("Article not found.");

        try
        {
            await _unitOfWork.BeginAsync();

            var oldTitle = article.title;
            var newSlug = GenerateSlug(title);
            article.UpdateContent(title, content, newSlug, thumbnailUrl);

            await _articleRepository.UpdateAsync(article);

            var auditLog = new AuditLog(staffId, "UPDATE_ARTICLE", "Articles", id.ToString())
            {
                oldValues = System.Text.Json.JsonSerializer.Serialize(new { title = oldTitle }),
                newValues = System.Text.Json.JsonSerializer.Serialize(new { title = title }),
                affectedColumns = "title,content,slug,thumbnailUrl"
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteArticleAsync(Guid id, Guid staffId)
    {
        var article = await _articleRepository.GetByIdAsync(id);
        if (article == null) throw new NotFoundException("Article not found.");

        try
        {
            await _unitOfWork.BeginAsync();

            await _articleRepository.DeleteAsync(id);

            var auditLog = new AuditLog(staffId, "DELETE_ARTICLE", "Articles", id.ToString())
            {
                oldValues = System.Text.Json.JsonSerializer.Serialize(new { title = article.title })
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task PublishArticleAsync(Guid id, Guid staffId)
    {
        var article = await _articleRepository.GetByIdAsync(id);
        if (article == null) throw new NotFoundException("Article not found.");

        try
        {
            await _unitOfWork.BeginAsync();

            article.Publish();
            await _articleRepository.UpdateAsync(article);

            var auditLog = new AuditLog(staffId, "PUBLISH_ARTICLE", "Articles", id.ToString())
            {
                oldValues = System.Text.Json.JsonSerializer.Serialize(new { status = ArticleStatus.DRAFT.ToString() }),
                newValues = System.Text.Json.JsonSerializer.Serialize(new { status = ArticleStatus.PUBLISHED.ToString() }),
                affectedColumns = "status"
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<(List<Article> items, int totalCount)> GetPagedArticlesAsync(int pageNumber, int pageSize)
    {
        return await _articleRepository.GetAllPagedAsync(pageNumber, pageSize);
    }

    private string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", " ").Trim();
        slug = slug.Replace(" ", "-");
        return slug;
    }
}
