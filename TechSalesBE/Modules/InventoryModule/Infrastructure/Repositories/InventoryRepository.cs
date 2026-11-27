using InventoryModule.Application.Repositories;
using InventoryModule.Domain.Entities;
using InventoryModule.Infrastructure.Persistence;
using InventoryModule.Infrastructure.Persistence.Models;
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

    public async Task EnsureStockExistsAsync(Guid productId, CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO inventory."Invetory" ("ProductId", "Quantity")
            VALUES ({productId}, 0)
            ON CONFLICT ("ProductId") DO NOTHING
            """, cancellationToken);
    }

    public async Task<Inventory?> GetStockForUpdateAsync(Guid productId, CancellationToken cancellationToken)
    {
        InvetoryDbModel? row = await _dbContext.Invetorys.FromSqlInterpolated($"""
            SELECT "ProductId", "Quantity", "ReservedQuantity"
            FROM inventory."Invetory"
            WHERE "ProductId" = {productId}
            FOR UPDATE
            """)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (row == null)
        {
            return null;
        }

        return new Inventory(row.ProductId, row.Quantity, row.ReservedQuantity);
    }

    public async Task UpdateStockQuantityAsync(Inventory stock, CancellationToken cancellationToken)
    {
        await _dbContext.Invetorys.Where(row => row.ProductId == stock.ProductId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(row => row.Quantity, stock.Quantity), cancellationToken);
    }

    public void AddProductUnits(List<ProductUnit> units)
    {
        foreach (ProductUnit unit in units)
        {
            _dbContext.ProductUnits.Add(new ProductUnitDbModel
            {
                ProductId = unit.ProductId,
                SerialNumber = unit.SerialNumber,
                Status = unit.Status
            });
        }
    }

    public async Task UpdateProductUnitStatusAsync(ProductUnit unit, CancellationToken cancellationToken)
    {
        await _dbContext.ProductUnits
            .Where(row => row.ProductId == unit.ProductId && row.SerialNumber == unit.SerialNumber)
            .ExecuteUpdateAsync(setters => setters.SetProperty(row => row.Status, unit.Status), cancellationToken);
    }
}
