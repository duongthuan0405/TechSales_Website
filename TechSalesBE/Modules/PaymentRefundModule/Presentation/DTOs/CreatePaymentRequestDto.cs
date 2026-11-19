using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Presentation.DTOs;

public sealed record CreatePaymentRequestDto(
    string? Type,
    Guid RefId,
    Guid TransactionMethodId);
