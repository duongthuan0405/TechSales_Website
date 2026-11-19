using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Domain.Rules;

namespace PaymentRefundModule.Domain.Entities;

internal sealed class Payment
{
    public Guid Id { get; }
    public PaymentType Type { get; }
    public Guid? OrderId { get; }
    public Guid? RepairRequestId { get; }
    public Guid TransactionMethodId { get; }
    public TransactionStatus Status { get; }
    public long Amount { get; }
    public string? TransactionRef { get; }
    public string? FailureReason { get; }
    public DateTimeOffset? CompletedAt { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; }

    internal Payment(Guid id, PaymentType type, Guid? orderId, Guid? repairRequestId,
        Guid transactionMethodId, TransactionStatus status, long amount, string? transactionRef,
        string? failureReason, DateTimeOffset? completedAt, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        Id = id;
        Type = type;
        OrderId = orderId;
        RepairRequestId = repairRequestId;
        TransactionMethodId = transactionMethodId;
        Status = status;
        Amount = amount;
        TransactionRef = transactionRef;
        FailureReason = failureReason;
        CompletedAt = completedAt;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Payment(PaymentType type, Guid refId, Guid transactionMethodId, long amount)
    {
        if (!PaymentRules.HasValidReference(type, refId))
        {
            throw new ArgumentException("Invalid payment reference.", nameof(refId));
        }

        if (transactionMethodId == Guid.Empty)
        {
            throw new ArgumentException("Invalid transaction method identifier.", nameof(transactionMethodId));
        }

        if (!PaymentRules.HasValidAmount(amount))
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        Id = Guid.NewGuid();
        Type = type;
        if (type == PaymentType.ORDER)
        {
            OrderId = refId;
        }
        else
        {
            RepairRequestId = refId;
        }

        TransactionMethodId = transactionMethodId;
        Status = TransactionStatus.PENDING;
        Amount = amount;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }
}
