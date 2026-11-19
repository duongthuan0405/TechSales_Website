using Microsoft.EntityFrameworkCore;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Infrastructure.Persistence;
using PaymentRefundModule.Infrastructure.Persistence.Models;

namespace PaymentRefundModule.Infrastructure.Repositories;

internal sealed class TransactionMethodRepository : ITransactionMethodRepository
{
    private readonly PaymentRefundDbContext _db;

    public TransactionMethodRepository(PaymentRefundDbContext db)
    {
        _db = db;
    }

    public async Task<List<TransactionMethod>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<TransactionMethodDbModel> rows = await _db.TransactionMethods.AsNoTracking()
            .OrderBy(item => item.Name).ThenBy(item => item.Id).ToListAsync(cancellationToken);
        List<TransactionMethod> transactionMethods = new List<TransactionMethod>();
        foreach (TransactionMethodDbModel row in rows)
        {
            transactionMethods.Add(MapToEntity(row));
        }

        return transactionMethods;
    }

    public async Task<TransactionMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        TransactionMethodDbModel? row = await _db.TransactionMethods.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (row == null)
        {
            return null;
        }

        return MapToEntity(row);
    }

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return _db.TransactionMethods.AnyAsync(cancellationToken);
    }

    private static TransactionMethod MapToEntity(TransactionMethodDbModel row)
    {
        // The domain requires a method type; never reinterpret missing data as CASH.
        if (!row.Type.HasValue)
        {
            throw new InvalidOperationException("TransactionMethod.Type is missing in persistence data.");
        }

        return new TransactionMethod(row.Id, row.Name ?? string.Empty, row.Type.Value);
    }
}
