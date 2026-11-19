using PaymentRefundModule.Application.Ports;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Application.Services;

internal sealed class PaymentOwnershipService
{
    private readonly IOrderOwnershipPort _orders;
    private readonly IRepairOwnershipPort _repairs;

    public PaymentOwnershipService(IOrderOwnershipPort orders, IRepairOwnershipPort repairs)
    {
        _orders = orders;
        _repairs = repairs;
    }

    public Task<bool> IsOwnedAsync(PaymentType type, Guid refId, Guid customerId, CancellationToken cancellationToken)
    {
        if (type == PaymentType.ORDER)
        {
            return _orders.IsOwnedByCustomerAsync(refId, customerId, cancellationToken);
        }

        if (type == PaymentType.REPAIR)
        {
            return _repairs.IsOwnedByCustomerAsync(refId, customerId, cancellationToken);
        }

        return Task.FromResult(false);
    }

    public Task<bool> IsOwnedAsync(Payment payment, Guid customerId, CancellationToken cancellationToken)
    {
        Guid? refId = payment.OrderId;
        if (payment.Type == PaymentType.REPAIR)
        {
            refId = payment.RepairRequestId;
        }

        if (!refId.HasValue)
        {
            return Task.FromResult(false);
        }

        return IsOwnedAsync(payment.Type, refId.Value, customerId, cancellationToken);
    }
}
