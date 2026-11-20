using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.UseCases.PollMomoPayment;

namespace PaymentRefundModule.Infrastructure.Payments.Momo;

internal sealed class MomoPollingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly MomoPollingOptions _options;
    private readonly ILogger<MomoPollingWorker> _logger;

    public MomoPollingWorker(IServiceScopeFactory scopes, IOptions<MomoPollingOptions> options,
        ILogger<MomoPollingWorker> logger)
    {
        _scopes = scopes;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        _logger.LogInformation("MoMo polling worker started. IntervalSeconds: {Interval}, BatchSize: {BatchSize}",
            _options.IntervalSeconds, _options.BatchSize);
        Guid? cursor = null;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                List<Guid> ids;
                await using (AsyncServiceScope scope = _scopes.CreateAsyncScope())
                {
                    ids = await scope.ServiceProvider.GetRequiredService<IMomoPollingSource>()
                        .GetPendingAsync(cursor, _options.BatchSize, stoppingToken);
                }
                foreach (Guid id in ids)
                {
                    try
                    {
                        // A separate scope prevents a failed transaction from affecting the next payment.
                        await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
                        PollMomoPayment_UCOutput result = await scope.ServiceProvider.GetRequiredService<ISender>()
                            .Send(new PollMomoPayment_UCInput(id), stoppingToken);
                        _logger.LogInformation("MoMo polling result. PaymentId: {PaymentId}, ResultCode: {ResultCode}, Updated: {Updated}",
                            id, result.ResultCode, result.Updated);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception exception)
                    {
                        _logger.LogWarning(exception, "MoMo polling failed. PaymentId: {PaymentId}", id);
                    }
                    cursor = id;
                    await Task.Delay(_options.RequestSpacingMilliseconds, stoppingToken);
                }
                if (ids.Count < _options.BatchSize)
                {
                    cursor = null;
                }
                await Task.Delay(TimeSpan.FromSeconds(_options.IntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "MoMo polling batch failed.");
                await Task.Delay(TimeSpan.FromSeconds(_options.IntervalSeconds), stoppingToken);
            }
        }
    }
}

