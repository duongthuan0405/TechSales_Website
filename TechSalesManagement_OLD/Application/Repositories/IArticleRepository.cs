using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Repositories;

public interface IArticleRepository
{
    Task AddAsync(Article article);
    Task UpdateAsync(Article article);
    Task DeleteAsync(Guid id);
    Task<Article?> GetByIdAsync(Guid id);
    Task<Article?> GetBySlugAsync(string slug);
    Task<(List<Article> items, int totalCount)> GetAllPagedAsync(int pageNumber, int pageSize);
}
