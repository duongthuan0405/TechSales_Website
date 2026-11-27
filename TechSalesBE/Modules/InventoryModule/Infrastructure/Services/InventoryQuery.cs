using InventoryModule.Contracts;
using InventoryModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventoryModule.Infrastructure.Services;

internal sealed class InventoryQuery : IInventoryQuery
{
    private readonly InventoryDbContext _db;

    public InventoryQuery(InventoryDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProductStock>> GetStocksAsync(List<Guid> productIds, CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
        {
            return new List<ProductStock>();
        }
        return await _db.Invetorys.AsNoTracking().Where(stock => productIds.Contains(stock.ProductId))
            .Select(stock => new ProductStock(stock.ProductId, stock.Quantity, stock.ReservedQuantity ?? 0))
            .ToListAsync(cancellationToken);
    }
}
