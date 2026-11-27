using Microsoft.EntityFrameworkCore;
using Npgsql;
using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Infrastructure.Persistence;
using PaymentRefundModule.Infrastructure.Services;
using Xunit;

namespace PaymentRefundModule.Tests;

public sealed class PaymentLockPostgresTests
{
    public const string ConnectionVariable = "PAYMENTREFUND_TEST_DB_CONNECTION";

    [PostgresFact]
    public async Task TryLockRejectsAnotherConnectionImmediatelyAndReleasesOnTransactionEnd()
    {
        var connection = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable(ConnectionVariable)!)
        { Timeout = 5, CommandTimeout = 5, Pooling = false };
        var options = new DbContextOptionsBuilder<PaymentRefundDbContext>().UseNpgsql(connection.ConnectionString).Options;
        await using var first = new PaymentRefundDbContext(options);
        await using var second = new PaymentRefundDbContext(options);
        await using var firstTransaction = await first.Database.BeginTransactionAsync();
        await using var secondTransaction = await second.Database.BeginTransactionAsync();
        var reference = Guid.NewGuid();
        var lock1 = new PaymentLockService(first);
        var lock2 = new PaymentLockService(second);
        await lock1.LockReferenceAsync(PaymentType.ORDER, reference); // Same key as create/replace.
        Assert.False(await lock2.TryLockReferenceAsync(PaymentType.ORDER, reference).WaitAsync(TimeSpan.FromSeconds(2)));
        Assert.True(await lock2.TryLockReferenceAsync(PaymentType.ORDER, Guid.NewGuid()));
        await firstTransaction.CommitAsync();
        Assert.True(await lock2.TryLockReferenceAsync(PaymentType.ORDER, reference));
        await secondTransaction.RollbackAsync();
        await using var nextTransaction = await first.Database.BeginTransactionAsync();
        Assert.True(await lock1.TryLockReferenceAsync(PaymentType.ORDER, reference));
        await nextTransaction.RollbackAsync();
    }

    public sealed class PostgresFactAttribute : FactAttribute
    {
        public PostgresFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConnectionVariable)))
                Skip = "Set PAYMENTREFUND_TEST_DB_CONNECTION to test actual PostgreSQL transaction locks.";
        }
    }
}
