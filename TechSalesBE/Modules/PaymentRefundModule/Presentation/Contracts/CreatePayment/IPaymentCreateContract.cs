namespace PaymentRefundModule.Presentation.Contracts.CreatePayment;

public interface IPaymentCreateContract
{
    Task<CreatePaymentResult> CreatePaymentAsync(
        Guid customerId,
        string paymentType,
        Guid refId,
        Guid transactionMethodId,
        CancellationToken cancellationToken = default);
}
