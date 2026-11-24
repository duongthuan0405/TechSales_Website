namespace InventoryModule.Application.Ports;

public interface IInventoryProducts
{
    Task<List<Guid>> GetExistingProductIdsAsync(List<Guid> productIds, CancellationToken cancellationToken);
}
