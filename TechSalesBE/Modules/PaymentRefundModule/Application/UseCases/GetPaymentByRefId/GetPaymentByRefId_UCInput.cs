using MediatR;
using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Application.UseCases.GetPaymentByRefId;

internal sealed record GetPaymentByRefId_UCInput(Guid CustomerId, Guid RefId, string? Type,
    bool CanViewAll = false, int PageNumber = 1, int PageSize = 20, string? Status = null) : IRequest<GetPaymentByRefId_UCOutput>;
