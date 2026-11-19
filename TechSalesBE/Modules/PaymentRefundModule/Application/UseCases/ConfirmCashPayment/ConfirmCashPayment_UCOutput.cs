namespace PaymentRefundModule.Application.UseCases.ConfirmCashPayment;

internal sealed record ConfirmCashPayment_UCOutput(Guid Id, string Status, DateTimeOffset? CompletedAt, DateTimeOffset UpdatedAt);
