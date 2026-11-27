using InventoryModule.Application.Ports;

namespace InventoryModule.Tests;

internal sealed class InventoryProductsFake : IInventoryProducts
{
    public HashSet<Guid> MissingIds { get; } = new HashSet<Guid>();
    public List<List<Guid>> Calls { get; } = new List<List<Guid>>();

    public Task<List<Guid>> GetExistingProductIdsAsync(List<Guid> productIds, CancellationToken cancellationToken)
    {
        Calls.Add(productIds.ToList());
        return Task.FromResult(productIds.Where(id => !MissingIds.Contains(id)).ToList());
    }
}
