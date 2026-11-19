using FluentValidation;
using PaymentRefundModule.Application.Exceptions;

namespace PaymentRefundModule.Application.UseCases.GetTransactionMethodById;

internal sealed class GetTransactionMethodById_UCInput_Validator : AbstractValidator<GetTransactionMethodById_UCInput>
{
    public GetTransactionMethodById_UCInput_Validator()
    {
        RuleFor(request => request.Id)
            .NotEmpty()
            .WithMessage(BusinessExceptionMessage.TransactionMethodIdInvalid);
    }
}
