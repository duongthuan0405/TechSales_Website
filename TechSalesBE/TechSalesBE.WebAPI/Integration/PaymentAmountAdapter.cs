using PaymentRefundModule.Application.Ports;
using PaymentRefundModule.Domain.Enums;

namespace TechSalesBE.WebAPI.Integration;

internal sealed class PaymentAmountAdapter : IPaymentAmountPort
{
    private readonly IConfiguration _configuration;

    public PaymentAmountAdapter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<long> GetAmountAsync(PaymentType type, Guid refId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // TODO: Replace these test amounts with the Order/Repair payment amount contracts.
        long amount = type switch
        {
            PaymentType.ORDER => _configuration.GetValue<long>("PaymentRefund:TestAmounts:Order", 100_000),
            PaymentType.REPAIR => _configuration.GetValue<long>("PaymentRefund:TestAmounts:Repair", 200_000),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
        return Task.FromResult(amount);
    }
}
