using FluentValidation;
using PaymentRefundModule.Application.Exceptions;
using PaymentRefundModule.Domain.Rules;

namespace PaymentRefundModule.Application.UseCases.CreatePayment;

internal sealed class CreatePayment_UCInput_Validator : AbstractValidator<CreatePayment_UCInput>
{
    public CreatePayment_UCInput_Validator()
    {
        RuleFor(request => request.CustomerId)
            .NotEmpty()
            .WithMessage(BusinessExceptionMessage.CustomerIdInvalid);
        RuleFor(request => request.Type)
            .Must(PaymentRules.ValidType)
            .WithMessage(BusinessExceptionMessage.PaymentTypeInvalid);

        RuleFor(request => request.RefId)
            .NotEmpty()
            .WithMessage(BusinessExceptionMessage.PaymentReferenceInvalid);

        RuleFor(request => request.TransactionMethodId)
            .NotEmpty()
            .WithMessage(BusinessExceptionMessage.TransactionMethodIdInvalid);
    }
}
