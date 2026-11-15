using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Repositories;

public interface ICategoryRepository
{
    Task AddAsync(Category category);
    Task DeleteAsync(Guid id);
    Task<Category?> GetByIdAsync(Guid id);
    Task<Category?> GetByNameAsync(string name);
    Task<List<Category>> GetAllAsync();
    Task<bool> ExistsAsync(Guid id);
}
