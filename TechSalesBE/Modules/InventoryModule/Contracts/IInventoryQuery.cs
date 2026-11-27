namespace InventoryModule.Contracts;

public interface IInventoryQuery
{
    Task<List<ProductStock>> GetStocksAsync(List<Guid> productIds, CancellationToken cancellationToken);
}

// Quantity is available stock; reserved units are tracked separately.
public sealed record ProductStock(Guid ProductId, int Quantity, int ReservedQuantity);
