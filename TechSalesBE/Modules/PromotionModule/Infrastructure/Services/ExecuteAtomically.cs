using Npgsql;
using CommonModule.BusinessExceptions;
using PromotionModule.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PromotionModule.Application.Services;
using PromotionModule.Infrastructure.Persistence;

namespace PromotionModule.Infrastructure.Services;

internal sealed class ExecuteAtomically : IExecuteAtomically
{
    private readonly PromotionDbContext _db;
    private readonly ILogger<ExecuteAtomically> _logger;

    public ExecuteAtomically(PromotionDbContext db, ILogger<ExecuteAtomically> logger)
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
        catch (Exception exception)
        {
            try
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch (Exception rollbackError)
            {
                _logger.LogWarning(rollbackError, "Promotion transaction rollback failed.");
            }

            _db.ChangeTracker.Clear();
            PostgresException? databaseException = exception.InnerException as PostgresException;
            if (exception is DbUpdateException && databaseException != null
                && databaseException.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.DuplicateCode);
            }

            throw;
        }
    }
}
