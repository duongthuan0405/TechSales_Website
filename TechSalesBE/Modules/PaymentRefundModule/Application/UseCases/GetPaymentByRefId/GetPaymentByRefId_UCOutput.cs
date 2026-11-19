namespace PaymentRefundModule.Application.UseCases.GetPaymentByRefId;

internal sealed record GetPaymentByRefId_UCOutput(List<GetPaymentByRefId_UCOutput.PaymentInfo> Items, int TotalCount, int PageNumber, int PageSize)
{
    internal sealed record PaymentInfo(Guid Id, string Type, Guid? OrderId, Guid? RepairRequestId, Guid TransactionMethodId, string Status, long Amount, string? TransactionRef, string? FailureReason, DateTimeOffset? CompletedAt, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
}
