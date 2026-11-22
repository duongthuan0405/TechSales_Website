using ProductModule.Application.Exceptions;
using ProductModule.Application.Services;
using ProductModule.Infrastructure.Persistence;
using CommonModule.BusinessExceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ProductModule.Infrastructure.Services;

internal sealed class ProductUnitOfWork : IProductUnitOfWork
{
    private readonly ProductDbContext _db;

    public ProductUnitOfWork(ProductDbContext db)
    {
        _db = db;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.ProductChanged);
        }
        catch (DbUpdateException exception)
        {
            PostgresException? postgresException = exception.InnerException as PostgresException;
            if (postgresException != null && postgresException.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                throw new BusinessException(BusinessExceptionCode.CONFLICT,
                    BusinessExceptionMessage.ProductReferencesChanged);
            }

            throw;
        }
    }
}
