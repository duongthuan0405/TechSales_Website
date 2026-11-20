namespace PaymentRefundModule.Application.Services.Payments;

internal interface IMomoStatusQuery
{
    Task<MomoStatusResult> QueryAsync(Guid paymentId, CancellationToken cancellationToken);
}

internal sealed record MomoStatusResult(string OrderId, long Amount, long TransId, int ResultCode);

