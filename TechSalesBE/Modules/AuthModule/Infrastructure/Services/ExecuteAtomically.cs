using AuthModule.Application.Exceptions;
using AuthModule.Application.Services;
using AuthModule.Infrastructure.Persistence;
using CommonModule.BusinessExceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AuthModule.Infrastructure.Services;
public sealed class ExecuteAtomically : IExecuteAtomically
{
    private readonly AuthDbContext _db;
    private readonly ILogger<ExecuteAtomically> _logger;

    public ExecuteAtomically(AuthDbContext db, ILogger<ExecuteAtomically> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<T> ExecuteAtomicallyAsync<T>(Func<Task<T>> mainTask, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await mainTask();
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (DbUpdateException exception)when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            await RollbackAsync();
            _db.ChangeTracker.Clear();
            var postgres = (PostgresException)exception.InnerException!;
            throw new BusinessException(BusinessExceptionCode.CONFLICT,
                postgres.ConstraintName == "IX_UserProfile_PhoneNumber" ? BusinessExceptionMessage.PhoneAlreadyUsed : BusinessExceptionMessage.DataConflict);
        }
        catch
        {
            await RollbackAsync();
            _db.ChangeTracker.Clear();
            throw;
        }

        async Task RollbackAsync()
        {
            try
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch (Exception rollbackError)
            {
                _logger.LogWarning(rollbackError, "Auth transaction rollback failed.");
            }
        }
    }
}
