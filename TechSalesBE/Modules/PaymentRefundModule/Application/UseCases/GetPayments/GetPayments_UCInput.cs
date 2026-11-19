using MediatR;
using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Application.UseCases.GetPayments;

internal sealed record GetPayments_UCInput(Guid CustomerId, int PageNumber = 1, int PageSize = 20,
    string? Type = null, string? Status = null, Guid? RefId = null) : IRequest<GetPayments_UCOutput>;
