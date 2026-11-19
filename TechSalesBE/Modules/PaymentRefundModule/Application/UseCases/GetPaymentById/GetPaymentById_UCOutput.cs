namespace PaymentRefundModule.Application.UseCases.GetPaymentById;

internal sealed record GetPaymentById_UCOutput(Guid Id, string Type, Guid? OrderId, Guid? RepairRequestId, Guid TransactionMethodId, string Status, long Amount, string? TransactionRef, string? FailureReason, DateTimeOffset? CompletedAt, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
