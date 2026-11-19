using MediatR;
using PaymentRefundModule.Domain.Enums;

namespace PaymentRefundModule.Application.UseCases.GetPaymentsForStaff;

internal sealed record GetPaymentsForStaff_UCInput(int PageNumber, int PageSize, string? Type,
    string? Status, Guid? RefId) : IRequest<GetPaymentsForStaff_UCOutput>;
