namespace PaymentRefundModule.Presentation.Contracts.ReplacePayment;

public sealed record ReplacePaymentResult(Guid PaymentId, Guid? ReplacedPaymentId);
