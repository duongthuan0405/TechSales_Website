using PaymentRefundModule.Domain.Enums;
using MediatR;

namespace PaymentRefundModule.Application.UseCases.CreatePayment;

internal sealed record CreatePayment_UCInput(
    Guid CustomerId,
    string? Type,
    Guid RefId,
    Guid TransactionMethodId) : IRequest<CreatePayment_UCOutput>;
