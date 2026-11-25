using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Interfaces;

public interface ICategoryService
{
    Task<Category> CreateCategoryAsync(string name, Guid staffId);
    Task DeleteCategoryAsync(Guid id, Guid replacementCategoryId, Guid staffId);
    Task<List<Category>> GetAllCategoriesAsync();
}
