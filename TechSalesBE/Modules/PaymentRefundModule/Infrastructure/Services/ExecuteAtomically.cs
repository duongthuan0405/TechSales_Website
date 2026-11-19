using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Infrastructure.Persistence;

namespace PaymentRefundModule.Infrastructure.Services;

internal sealed class ExecuteAtomically : IExecuteAtomically
{
    private readonly PaymentRefundDbContext _db;
    private readonly ILogger<ExecuteAtomically> _logger;

    public ExecuteAtomically(PaymentRefundDbContext db, ILogger<ExecuteAtomically> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<T> ExecuteAtomicallyAsync<T>(Func<Task<T>> mainTask, CancellationToken cancellationToken = default)
    {
        await using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            T result = await mainTask();
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            try
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch (Exception rollbackError)
            {
                _logger.LogWarning(rollbackError, "Payment/refund transaction rollback failed.");
            }

            _db.ChangeTracker.Clear();
            throw;
        }
    }
}
