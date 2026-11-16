using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Infrastructure.Persistence.Models;

internal sealed class PaymentDbModel
{
    public Guid Id { get; set; }
    public PaymentType? Type { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? RepairRequestId { get; set; }
    public Guid TransactionMethodId { get; set; }
    public TransactionStatus Status { get; set; }
    public long Amount { get; set; }
    public string? TransactionRef { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public TransactionMethodDbModel TransactionMethod { get; set; } = null!;
}
