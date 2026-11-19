namespace PaymentRefundModule.Presentation.DTOs;

public sealed record ReplacePaymentRequestDto(
    string? Type,
    Guid RefId,
    Guid TransactionMethodId);
