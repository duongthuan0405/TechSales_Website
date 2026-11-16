using PaymentRefundModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace PaymentRefundModule.Infrastructure.Persistence;

public sealed class PaymentRefundDbContext : DbContext
{
    internal DbSet<TransactionMethodDbModel> TransactionMethods => Set<TransactionMethodDbModel>();
    internal DbSet<PaymentDbModel> Payments => Set<PaymentDbModel>();
    internal DbSet<RefundDbModel> Refunds => Set<RefundDbModel>();

    public const string Schema = "payment_refund";

    public PaymentRefundDbContext(DbContextOptions<PaymentRefundDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PaymentRefundDbContext).Assembly,
            type => type.Namespace == "PaymentRefundModule.Infrastructure.Persistence.Configurations"
                || (type.Namespace?.StartsWith("PaymentRefundModule.Infrastructure.Persistence.Configurations.", StringComparison.Ordinal) ?? false));
    }
}