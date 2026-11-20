namespace PaymentRefundModule.Application.Repositories;

internal interface IMomoPollingSource
{
    Task<List<Guid>> GetPendingAsync(Guid? afterId, int batchSize, CancellationToken cancellationToken);
}

