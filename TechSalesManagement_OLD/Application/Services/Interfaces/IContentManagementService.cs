using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Services.Interfaces;

public interface IContentManagementService
{
    Task<Article> CreateArticleAsync(string title, string content, string? thumbnailUrl, Guid authorId);
    Task UpdateArticleAsync(Guid id, string title, string content, string? thumbnailUrl, Guid staffId);
    Task DeleteArticleAsync(Guid id, Guid staffId);
    Task PublishArticleAsync(Guid id, Guid staffId);
    Task<(List<Article> items, int totalCount)> GetPagedArticlesAsync(int pageNumber, int pageSize);
}
