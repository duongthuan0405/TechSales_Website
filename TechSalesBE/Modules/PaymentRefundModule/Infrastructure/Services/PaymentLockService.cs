using Microsoft.EntityFrameworkCore;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Infrastructure.Persistence;

namespace PaymentRefundModule.Infrastructure.Services;

internal sealed class PaymentLockService : IPaymentLockService
{
    private readonly PaymentRefundDbContext _db;

    public PaymentLockService(PaymentRefundDbContext db)
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
}
