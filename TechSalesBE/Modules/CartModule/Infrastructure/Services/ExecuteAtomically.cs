using System.Data;
using CartModule.Application.Services;
using CartModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace CartModule.Infrastructure.Services;

internal sealed class ExecuteAtomically : IExecuteAtomically
{
    private readonly CartDbContext _dbContext;
    private readonly ILogger<ExecuteAtomically> _logger;

    public ExecuteAtomically(CartDbContext dbContext, ILogger<ExecuteAtomically> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<T> ExecuteAtomicallyAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        await using IDbContextTransaction transaction =
            await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            T result = await action();
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
                _logger.LogWarning(rollbackError, "Cart transaction rollback failed.");
            }

            throw;
        }
    }
}
