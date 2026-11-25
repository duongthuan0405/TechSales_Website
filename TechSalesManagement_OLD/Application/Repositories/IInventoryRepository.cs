using System;
using System.Threading.Tasks;

namespace TechSalesManagement.Application.Repositories;

public interface IInventoryRepository
{
    Task ReserveStockAsync(Guid productId, int quantity);
    Task ReleaseStockAsync(Guid productId, int quantity);
    Task DeductStockAsync(Guid productId, int quantity);
    Task RestorePhysicalStockAsync(Guid productId, int quantity);
    Task<TechSalesManagement.Domain.Entities.Inventory?> GetByProductIdAsync(Guid productId);
    Task UpdateStockAsync(Guid productId, int quantity);
}
