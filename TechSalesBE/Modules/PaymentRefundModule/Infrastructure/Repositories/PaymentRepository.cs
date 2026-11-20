using PaymentRefundModule.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Infrastructure.Persistence;
using PaymentRefundModule.Infrastructure.Persistence.Models;

namespace PaymentRefundModule.Infrastructure.Repositories;

internal sealed class PaymentRepository : IPaymentRepository
{
    private readonly PaymentRefundDbContext _db;

    public PaymentRepository(PaymentRefundDbContext db)
    {
        _db = db;
    }

    public async Task LockReferenceAsync(PaymentType type, Guid refId, CancellationToken cancellationToken = default)
    {
        if (_db.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException("Payment reference locking requires an active transaction.");
        }

        string lockKey = $"payment:{type}:{refId:D}";
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))", cancellationToken);
    }

    public async Task<Payment?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_db.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException("Payment row locking requires an active transaction.");
        }

        PaymentDbModel? row = await _db.Payments
            .FromSqlInterpolated($"SELECT * FROM payment_refund.\"Payment\" WHERE \"Id\" = {id} FOR UPDATE")
            .Include(item => item.TransactionMethod).AsNoTracking().SingleOrDefaultAsync(cancellationToken);
        if (row == null)
        {
            return null;
        }

        return MapToEntity(row);
    }

    public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        PaymentDbModel row = await _db.Payments.SingleAsync(item => item.Id == payment.Id, cancellationToken);
        row.Status = payment.Status;
        row.TransactionRef = payment.TransactionRef;
        row.CompletedAt = payment.CompletedAt;
        row.UpdatedAt = payment.UpdatedAt;
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        PaymentDbModel? row = await _db.Payments.Include(item => item.TransactionMethod)
            .AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (row == null)
        {
            return null;
        }

        return MapToEntity(row);
    }

    public async Task<List<Payment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<PaymentDbModel> rows = await _db.Payments.Include(item => item.TransactionMethod).AsNoTracking()
            .OrderByDescending(item => item.CreatedAt).ThenBy(item => item.Id).ToListAsync(cancellationToken);
        return rows.Select(MapToEntity).ToList();
    }

    public async Task<(List<Payment> Items, int TotalCount)> GetPageAsync(int pageNumber, int pageSize,
        PaymentType? type, TransactionStatus? status, Guid? refId, CancellationToken cancellationToken = default)
    {
        IQueryable<PaymentDbModel> query = _db.Payments.AsNoTracking();
        if (type.HasValue)
        {
            query = query.Where(item => item.Type == type.Value);
        }
        if (status.HasValue)
        {
            query = query.Where(item => item.Status == status.Value);
        }
        if (refId.HasValue)
        {
            query = query.Where(item => (item.Type == PaymentType.ORDER && item.OrderId == refId.Value)
                || (item.Type == PaymentType.REPAIR && item.RepairRequestId == refId.Value));
        }

        int totalCount = await query.CountAsync(cancellationToken);
        List<PaymentDbModel> rows = await query.Include(item => item.TransactionMethod)
            .OrderByDescending(item => item.CreatedAt).ThenBy(item => item.Id)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (rows.Select(MapToEntity).ToList(), totalCount);
    }

    public async Task<List<Payment>> GetByRefIdAsync(Guid refId, PaymentType type, CancellationToken cancellationToken = default)
    {
        IQueryable<PaymentDbModel> query = _db.Payments.AsNoTracking().Where(item => item.Type == type);
        if (type == PaymentType.ORDER)
        {
            query = query.Where(item => item.OrderId == refId);
        }
        else
        {
            query = query.Where(item => item.RepairRequestId == refId);
        }

        List<PaymentDbModel> rows = await query.Include(item => item.TransactionMethod).OrderByDescending(item => item.CreatedAt)
            .ThenBy(item => item.Id).ToListAsync(cancellationToken);
        return rows.Select(MapToEntity).ToList();
    }

    private static Payment MapToEntity(PaymentDbModel row)
    {
        if (!row.Type.HasValue)
        {
            throw new InvalidOperationException("Payment.Type is missing in persistence data.");
        }

        if (row.TransactionMethod == null || !row.TransactionMethod.Type.HasValue)
        {
            throw new InvalidOperationException("TransactionMethod or TransactionMethod.Type is missing in persistence data.");
        }

        TransactionMethod method = new TransactionMethod(row.TransactionMethod.Id,
            row.TransactionMethod.Name ?? string.Empty, row.TransactionMethod.Type.Value, row.TransactionMethod.Code);
        return new Payment(row.Id, row.Type.Value, row.OrderId, row.RepairRequestId,
            row.TransactionMethodId, row.Status, row.Amount, row.TransactionRef, row.FailureReason,
            row.CompletedAt, row.CreatedAt, row.UpdatedAt, method);
    }

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        PaymentDbModel row = new PaymentDbModel
        {
            Id = payment.Id,
            Type = payment.Type,
            OrderId = payment.OrderId,
            RepairRequestId = payment.RepairRequestId,
            TransactionMethodId = payment.TransactionMethodId,
            Status = payment.Status,
            Amount = payment.Amount,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt
        };
        await _db.Payments.AddAsync(row, cancellationToken);
    }
}
