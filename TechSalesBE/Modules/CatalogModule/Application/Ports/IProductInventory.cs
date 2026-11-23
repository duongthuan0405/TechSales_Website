namespace CatalogModule.Application.Ports;

public interface IProductInventory
{
    Task<List<ProductInventory>> GetStocksAsync(List<Guid> productIds, CancellationToken cancellationToken);
}

public sealed record ProductInventory(Guid ProductId, int Quantity, int ReservedQuantity);
