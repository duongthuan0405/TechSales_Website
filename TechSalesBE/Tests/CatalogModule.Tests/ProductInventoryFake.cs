using CatalogModule.Application.Ports;

namespace CatalogModule.Tests;

internal sealed class ProductInventoryFake : IProductInventory
{
    public List<ProductInventory> Stocks { get; } = new List<ProductInventory>();

    public Task<List<ProductInventory>> GetStocksAsync(List<Guid> productIds, CancellationToken cancellationToken)
    {
        return Task.FromResult(Stocks.Where(stock => productIds.Contains(stock.ProductId)).ToList());
    }
}
