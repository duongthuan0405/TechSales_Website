using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Application.Services;

internal interface IPaymentLockService
{
    Task LockReferenceAsync(PaymentType type, Guid refId, CancellationToken cancellationToken = default);
}
