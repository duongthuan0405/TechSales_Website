using PaymentRefundModule.Application.Ports;

namespace TechSalesBE.WebAPI.Integration;

internal sealed class OrderOwnershipAdapter : IOrderOwnershipPort
{
    public Task<bool> IsOwnedByCustomerAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // TODO: Replace this temporary result with the OrderingModule ownership contract.
        return Task.FromResult(true);
    }
}
