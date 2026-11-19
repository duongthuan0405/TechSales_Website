using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Application.Ports;

public interface IPaymentAmountPort
{
    Task<long> GetAmountAsync(PaymentType type, Guid refId, CancellationToken cancellationToken = default);
}
