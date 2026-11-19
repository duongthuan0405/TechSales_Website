using MediatR;
using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Application.UseCases.GetPaymentById;

internal sealed record GetPaymentById_UCInput(Guid CustomerId, Guid Id, bool CanViewAll = false) : IRequest<GetPaymentById_UCOutput>;
