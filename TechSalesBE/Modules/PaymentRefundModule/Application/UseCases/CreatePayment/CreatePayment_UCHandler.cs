using CommonModule.BusinessExceptions;
using MediatR;
using PaymentRefundModule.Application.Exceptions;
using PaymentRefundModule.Application.Ports;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Domain.Rules;

namespace PaymentRefundModule.Application.UseCases.CreatePayment;

internal sealed class CreatePayment_UCHandler : IRequestHandler<CreatePayment_UCInput, CreatePayment_UCOutput>
{
    private readonly IPaymentRepository _payments;
    private readonly ITransactionMethodRepository _methods;
    private readonly IOrderOwnershipPort _orders;
    private readonly IRepairOwnershipPort _repairs;
    private readonly IPaymentAmountPort _amounts;
    private readonly IExecuteAtomically _transaction;

    public CreatePayment_UCHandler(IPaymentRepository payments, ITransactionMethodRepository methods,
        IOrderOwnershipPort orders, IRepairOwnershipPort repairs, IPaymentAmountPort amounts,
        IExecuteAtomically transaction)
    {
        _payments = payments;
        _methods = methods;
        _orders = orders;
        _repairs = repairs;
        _amounts = amounts;
        _transaction = transaction;
    }

    public async Task<CreatePayment_UCOutput> Handle(CreatePayment_UCInput request, CancellationToken cancellationToken)
    {
        PaymentType type = PaymentInputParser.RequiredType(request.Type);
        if (!PaymentRules.HasValidReference(type, request.RefId))
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.PaymentReferenceInvalid);
        }

        bool isOwner;
        if (type == PaymentType.ORDER)
        {
            isOwner = await _orders.IsOwnedByCustomerAsync(request.RefId, request.CustomerId, cancellationToken);
        }
        else
        {
            isOwner = await _repairs.IsOwnedByCustomerAsync(request.RefId, request.CustomerId, cancellationToken);
        }

        if (!isOwner)
        {
            throw new BusinessException(BusinessExceptionCode.FORBIDDEN, BusinessExceptionMessage.PaymentNotOwned);
        }

        long amount = await _amounts.GetAmountAsync(type, request.RefId, cancellationToken);
        if (!PaymentRules.HasValidAmount(amount))
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.PaymentAmountInvalid);
        }

        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            TransactionMethod? method = await _methods.GetByIdAsync(request.TransactionMethodId, cancellationToken);
            if (method == null)
            {
                throw new BusinessException(BusinessExceptionCode.NOT_FOUND, BusinessExceptionMessage.TransactionMethodNotFound);
            }

            Payment payment = new Payment(type, request.RefId, method.Id, amount);
            await _payments.AddAsync(payment, cancellationToken);
            return new CreatePayment_UCOutput(payment.Id);
        }, cancellationToken);
    }
}
