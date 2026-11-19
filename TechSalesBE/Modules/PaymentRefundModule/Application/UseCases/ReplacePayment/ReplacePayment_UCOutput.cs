namespace PaymentRefundModule.Application.UseCases.ReplacePayment;

internal sealed record ReplacePayment_UCOutput(Guid PaymentId, Guid? ReplacedPaymentId);
