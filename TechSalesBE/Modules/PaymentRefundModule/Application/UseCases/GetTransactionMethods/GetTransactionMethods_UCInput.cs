using MediatR;

namespace PaymentRefundModule.Application.UseCases.GetTransactionMethods;

internal sealed record GetTransactionMethods_UCInput() : IRequest<GetTransactionMethods_UCOutput>;

