using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using ShippingModule.Application.Services;
using ShippingModule.Infrastructure.Persistence;

namespace ShippingModule.Infrastructure.Services;

internal sealed class ExecuteAtomically : IExecuteAtomically
{
    private readonly ShippingDbContext _db;
    private readonly ILogger<ExecuteAtomically> _logger;

    public ExecuteAtomically(ShippingDbContext db, ILogger<ExecuteAtomically> logger)
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
                _logger.LogWarning(rollbackError, "Shipping transaction rollback failed.");
            }

            _db.ChangeTracker.Clear();
            throw;
        }
    }
}
