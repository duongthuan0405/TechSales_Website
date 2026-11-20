using Microsoft.EntityFrameworkCore;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Infrastructure.Persistence;

namespace PaymentRefundModule.Infrastructure.Payments.Momo;

internal sealed class MomoPollingSource : IMomoPollingSource
{
    private readonly PaymentRefundDbContext _db;

    public MomoPollingSource(PaymentRefundDbContext db)
    {
        _db = db;
    }

    public Task<List<Guid>> GetPendingAsync(Guid? afterId, int batchSize, CancellationToken cancellationToken)
    {
        var query = _db.Payments.AsNoTracking().Where(payment =>
            payment.Status == TransactionStatus.PENDING &&
            payment.TransactionMethod != null &&
            payment.TransactionMethod.Type == TransactionMethodType.ONLINE &&
            payment.TransactionMethod.Code != null &&
            payment.TransactionMethod.Code.Trim().ToUpper() == "MOMO");
        if (afterId.HasValue)
        {
            Guid cursor = afterId.Value;
            query = query.Where(payment => payment.Id.CompareTo(cursor) > 0);
        }

        return query.OrderBy(payment => payment.Id).Select(payment => payment.Id)
            .Take(batchSize).ToListAsync(cancellationToken);
    }
}

