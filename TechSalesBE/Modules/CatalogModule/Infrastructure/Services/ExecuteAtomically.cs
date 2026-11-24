using CatalogModule.Application.Exceptions;
using CatalogModule.Application.Services;
using CatalogModule.Infrastructure.Persistence;
using CommonModule.BusinessExceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CatalogModule.Infrastructure.Services;

internal sealed class ExecuteAtomically : IExecuteAtomically
{
    private const string BrandNameUniqueConstraint = "IX_Brand_Name";

    private readonly CatalogDbContext _db;
    private readonly ILogger<ExecuteAtomically> _logger;

    public ExecuteAtomically(CatalogDbContext db, ILogger<ExecuteAtomically> logger)
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
                _logger.LogWarning(rollbackError, "Catalog transaction rollback failed.");
            }

            _db.ChangeTracker.Clear();
            if (IsBrandNameUniqueViolation(exception))
            {
                throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.DuplicateBrandName);
            }

            throw;
        }
    }

    private static bool IsBrandNameUniqueViolation(Exception exception)
    {
        return exception is DbUpdateException
            && exception.InnerException is PostgresException databaseException
            && databaseException.SqlState == PostgresErrorCodes.UniqueViolation
            && databaseException.ConstraintName == BrandNameUniqueConstraint;
    }
}
