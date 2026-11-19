using MediatR;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Domain.Entities;

namespace PaymentRefundModule.Application.UseCases.GetTransactionMethods;

internal sealed class GetTransactionMethods_UCHandler : IRequestHandler<GetTransactionMethods_UCInput, GetTransactionMethods_UCOutput>
{
    private readonly ITransactionMethodRepository _transactionMethodRepo;
    private readonly IExecuteAtomically _executeAtomically;
    public GetTransactionMethods_UCHandler(
        ITransactionMethodRepository transactionMethodRepository,
        IExecuteAtomically executeAtomically
    )
    {
        _transactionMethodRepo = transactionMethodRepository;
        _executeAtomically = executeAtomically;
    }
    public async Task<GetTransactionMethods_UCOutput> Handle(GetTransactionMethods_UCInput request, CancellationToken cancellationToken)
    {
        return await _executeAtomically.ExecuteAtomicallyAsync(async () =>
        {
           List<TransactionMethod> transactionMethods = await _transactionMethodRepo.GetAllAsync(cancellationToken);

           var list = transactionMethods.Select(t =>
           {
               return new GetTransactionMethods_UCOutput.TransactionMethodInfo(t.Id.ToString(), t.Name, t.Type.ToString(), t.Code);
           }).ToList();
           return new GetTransactionMethods_UCOutput(list); 
        });
    } 
}
