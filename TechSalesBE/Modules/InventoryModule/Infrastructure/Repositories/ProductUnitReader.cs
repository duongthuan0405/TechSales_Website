using InventoryModule.Application.Repositories;
using InventoryModule.Domain.Entities;
using InventoryModule.Domain.Enums;
using InventoryModule.Infrastructure.Persistence;
using InventoryModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryModule.Infrastructure.Repositories;

internal sealed class ProductUnitReader : IProductUnitReader
{
    private readonly InventoryDbContext _dbContext;

    public ProductUnitReader(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(List<ProductUnit> Items, int TotalCount)> BrowseAsync(
        Guid? productId, string? keyword, string? status, int pageNumber, int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<ProductUnitDbModel> query = _dbContext.ProductUnits.AsNoTracking();
        if (productId.HasValue)
        {
            query = query.Where(unit => unit.ProductId == productId.Value);
        }
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string search = keyword.Trim();
            query = query.Where(unit => unit.SerialNumber.Contains(search));
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            ProductUnitStatus parsedStatus = Enum.Parse<ProductUnitStatus>(status);
            query = query.Where(unit => unit.Status == parsedStatus);
        }
        int totalCount = await query.CountAsync(cancellationToken);
        List<ProductUnitDbModel> rows = await query.OrderBy(unit => unit.ProductId)
            .ThenBy(unit => unit.SerialNumber).Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
        List<ProductUnit> items = rows.Select(row => new ProductUnit(row.ProductId, row.SerialNumber, row.Status)).ToList();
        return (items, totalCount);
    }
}
