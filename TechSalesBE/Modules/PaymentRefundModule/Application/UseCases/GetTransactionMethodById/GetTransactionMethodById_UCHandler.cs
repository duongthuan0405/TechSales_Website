using CommonModule.BusinessExceptions;
using MediatR;
using PaymentRefundModule.Application.Exceptions;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Domain.Entities;

namespace PaymentRefundModule.Application.UseCases.GetTransactionMethodById;

internal sealed class GetTransactionMethodById_UCHandler : IRequestHandler<GetTransactionMethodById_UCInput, GetTransactionMethodById_UCOutput>
{
    private readonly ITransactionMethodRepository _transactionMethodRepo;
    private readonly IExecuteAtomically _executeAtomically;
    public GetTransactionMethodById_UCHandler(
        ITransactionMethodRepository transactionMethodRepository,
        IExecuteAtomically executeAtomically
    )
    {
        _transactionMethodRepo = transactionMethodRepository;
        _executeAtomically = executeAtomically;
    }
    public async Task<GetTransactionMethodById_UCOutput> Handle(GetTransactionMethodById_UCInput request, CancellationToken cancellationToken)
    {
        return await _executeAtomically.ExecuteAtomicallyAsync(async () =>
        {
           var transactionMethod = await _transactionMethodRepo.GetByIdAsync(request.Id);
            if(transactionMethod is null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.TransactionMethodNotFound);
            }

            return new GetTransactionMethodById_UCOutput(transactionMethod.Id.ToString(), transactionMethod.Name, transactionMethod.Type.ToString(), transactionMethod.Code);
        });
    } 
}
