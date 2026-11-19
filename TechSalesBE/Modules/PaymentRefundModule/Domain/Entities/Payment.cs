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
    public TransactionMethod? TransactionMethod { get; }
    public TransactionStatus Status { get; private set; }
    public long Amount { get; }
    public string? TransactionRef { get; private set; }
    public string? FailureReason { get; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public bool ConfirmCash(TransactionMethodType methodType)
    {
        PaymentRules.EnsureCashConfirmationAllowed(methodType, Status);
        if (Status == TransactionStatus.SUCCESS)
        {
            return false;
        }

        Status = TransactionStatus.SUCCESS;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CompletedAt.Value;
        return true;
    }

    public bool ConfirmOnline(string transactionRef)
    {
        PaymentRules.EnsureOnlineConfirmationAllowed(TransactionMethod, Status, TransactionRef, transactionRef);
        if (Status == TransactionStatus.SUCCESS)
        {
            return false;
        }

        Status = TransactionStatus.SUCCESS;
        TransactionRef = transactionRef;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CompletedAt.Value;
        return true;
    }

    public void CancelForReplacement(TransactionMethodType methodType)
    {
        PaymentRules.EnsureReplacementAllowed(Status, methodType);
        Status = TransactionStatus.CANCELLED;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    internal Payment(Guid id, PaymentType type, Guid? orderId, Guid? repairRequestId,
        Guid transactionMethodId, TransactionStatus status, long amount, string? transactionRef,
        string? failureReason, DateTimeOffset? completedAt, DateTimeOffset createdAt, DateTimeOffset updatedAt,
        TransactionMethod? transactionMethod = null)
    {
        Id = id;
        Type = type;
        OrderId = orderId;
        RepairRequestId = repairRequestId;
        TransactionMethodId = transactionMethodId;
        TransactionMethod = transactionMethod;
        Status = status;
        Amount = amount;
        TransactionRef = transactionRef;
        FailureReason = failureReason;
        CompletedAt = completedAt;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Payment(PaymentType type, Guid refId, Guid transactionMethodId, long amount, TransactionMethod? transactionMethod = null)
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
        TransactionMethod = transactionMethod;
        Status = TransactionStatus.PENDING;
        Amount = amount;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }
}
