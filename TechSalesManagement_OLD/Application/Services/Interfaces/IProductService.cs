using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Application.Services.Params;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Interfaces;

public interface IProductService
{
    Task<List<Product>> SearchProductsAsync(SearchProductParams parameters);
    Task<Product> GetProductDetailsAsync(GetProductDetailsParams parameters);
}
