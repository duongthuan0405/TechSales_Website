namespace PaymentRefundModule.Presentation.Contracts.ReplacePayment;

public interface IPaymentReplaceContract
{
    Task<ReplacePaymentResult> ReplacePaymentAsync(
        Guid customerId,
        string paymentType,
        Guid refId,
        Guid transactionMethodId,
        CancellationToken cancellationToken = default);
}
