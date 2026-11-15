using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Infrastructure.Persistence;

namespace TechSalesManagement.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly TechSalesDbContext _dbContext;

    public InventoryRepository(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ReserveStockAsync(Guid productId, int quantityToReserve)
    {
        var dbModel = await _dbContext.Inventories
            .FirstOrDefaultAsync(i => i.product_id == productId);

        if (dbModel != null)
        {
            dbModel.reserved_quantity += quantityToReserve;
            _dbContext.Inventories.Update(dbModel);
        }
    }

    public async Task ReleaseStockAsync(Guid productId, int quantityToRelease)
    {
        var dbModel = await _dbContext.Inventories
            .FirstOrDefaultAsync(i => i.product_id == productId);

        if (dbModel != null)
        {
            dbModel.reserved_quantity = Math.Max(0, dbModel.reserved_quantity - quantityToRelease);
            _dbContext.Inventories.Update(dbModel);
        }
    }

    public async Task DeductStockAsync(Guid productId, int quantityToDeduct)
    {
        var dbModel = await _dbContext.Inventories
            .FirstOrDefaultAsync(i => i.product_id == productId);

        if (dbModel != null)
        {
            dbModel.quantity = Math.Max(0, dbModel.quantity - quantityToDeduct);
            dbModel.reserved_quantity = Math.Max(0, dbModel.reserved_quantity - quantityToDeduct);
            _dbContext.Inventories.Update(dbModel);
        }
    }

    public async Task RestorePhysicalStockAsync(Guid productId, int quantityToRestore)
    {
        var dbModel = await _dbContext.Inventories
            .FirstOrDefaultAsync(i => i.product_id == productId);

        if (dbModel != null)
        {
            dbModel.quantity += quantityToRestore;
            _dbContext.Inventories.Update(dbModel);
        }
    }

    public async Task<TechSalesManagement.Domain.Entities.Inventory?> GetByProductIdAsync(Guid productId)
    {
        var dbModel = await _dbContext.Inventories
            .FirstOrDefaultAsync(i => i.product_id == productId);

        if (dbModel == null) return null;

        return new TechSalesManagement.Domain.Entities.Inventory
        {
            productId = dbModel.product_id,
            quantity = dbModel.quantity,
            reservedQuantity = dbModel.reserved_quantity
        };
    }

    public async Task UpdateStockAsync(Guid productId, int quantity)
    {
        var dbModel = await _dbContext.Inventories
            .FirstOrDefaultAsync(i => i.product_id == productId);

        if (dbModel != null)
        {
            dbModel.quantity = quantity;
            _dbContext.Inventories.Update(dbModel);
        }
    }
}
