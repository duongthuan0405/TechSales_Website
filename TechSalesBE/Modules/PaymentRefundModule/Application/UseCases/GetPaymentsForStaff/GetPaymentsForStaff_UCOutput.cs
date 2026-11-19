namespace PaymentRefundModule.Application.UseCases.GetPaymentsForStaff;

internal sealed record GetPaymentsForStaff_UCOutput(List<GetPaymentsForStaff_UCOutput.PaymentInfo> Items,
    int TotalCount, int PageNumber, int PageSize)
{
    internal sealed record PaymentInfo(Guid Id, string Type, Guid? OrderId, Guid? RepairRequestId,
        Guid TransactionMethodId, string Status, long Amount, string? TransactionRef,
        string? FailureReason, DateTimeOffset? CompletedAt, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
}
