using PaymentRefundModule.Application.Ports;

namespace TechSalesBE.WebAPI.Integration;

internal sealed class RepairOwnershipAdapter : IRepairOwnershipPort
{
    public Task<bool> IsOwnedByCustomerAsync(Guid repairRequestId, Guid customerId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // TODO: Replace this temporary result with the repair request ownership contract.
        return Task.FromResult(true);
    }
}
