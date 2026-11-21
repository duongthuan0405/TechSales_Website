using CommonModule.BusinessExceptions;
using InventoryModule.Application.Exceptions;
using InventoryModule.Application.Services;
using InventoryModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace InventoryModule.Infrastructure.Services;

internal sealed class ExecuteAtomically : IExecuteAtomically
{
    private readonly InventoryDbContext _dbContext;

    public ExecuteAtomically(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken)
    {
        await using IDbContextTransaction transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            T result = await operation();
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            SchemaName: InventoryDbContext.Schema,
            TableName: "ProductUnit"
        })
        {
            throw new BusinessException(
                BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.SerialNumberAlreadyExists);
        }
        finally
        {
            _dbContext.ChangeTracker.Clear();
        }
    }
}
