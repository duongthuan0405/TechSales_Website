namespace PaymentRefundModule.Application.Ports;

public interface IRepairOwnershipPort
{
    Task<bool> IsOwnedByCustomerAsync(Guid repairRequestId, Guid customerId, CancellationToken cancellationToken = default);
}
