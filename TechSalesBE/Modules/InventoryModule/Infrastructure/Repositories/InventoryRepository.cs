using InventoryModule.Application.Repositories;
using InventoryModule.Domain.Entities;
using InventoryModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventoryModule.Infrastructure.Repositories;

internal sealed class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _dbContext;

    public InventoryRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Inventory?> GetStockAsync(Guid productId, CancellationToken cancellationToken)
    {
        var row = await _dbContext.Invetorys.AsNoTracking()
            .SingleOrDefaultAsync(stock => stock.ProductId == productId, cancellationToken);

        if (row == null)
        {
            return null;
        }

        return new Inventory(row.ProductId, row.Quantity, row.ReservedQuantity);
    }

    public async Task<ProductUnit?> GetProductUnitAsync(
        Guid productId, string serialNumber, CancellationToken cancellationToken)
    {
        var row = await _dbContext.ProductUnits.AsNoTracking()
            .SingleOrDefaultAsync(
                unit => unit.ProductId == productId && unit.SerialNumber == serialNumber,
                cancellationToken);

        if (row == null)
        {
            return null;
        }

        return new ProductUnit(row.ProductId, row.SerialNumber, row.Status);
    }
}
