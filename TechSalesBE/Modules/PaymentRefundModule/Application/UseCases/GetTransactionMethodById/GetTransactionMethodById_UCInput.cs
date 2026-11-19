using MediatR;

namespace PaymentRefundModule.Application.UseCases.GetTransactionMethodById;

internal sealed record GetTransactionMethodById_UCInput(Guid Id) : IRequest<GetTransactionMethodById_UCOutput>;

