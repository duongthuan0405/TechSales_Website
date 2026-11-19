namespace PaymentRefundModule.Application.Ports;

public interface IOrderOwnershipPort
{
    Task<bool> IsOwnedByCustomerAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken = default);
}
