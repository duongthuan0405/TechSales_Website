using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Infrastructure.Persistence.Models;

internal sealed class RefundDbModel
{
    public Guid Id { get; set; }
    public RefundType? Type { get; set; }
    public Guid? ReturnRequestId { get; set; }
    public Guid? OrderId { get; set; }
    public TransactionStatus Status { get; set; }
    public long Amount { get; set; }
    public string? RefundTransactionRef { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
