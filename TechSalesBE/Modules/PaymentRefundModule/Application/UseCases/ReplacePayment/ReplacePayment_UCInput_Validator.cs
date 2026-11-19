using FluentValidation;
using PaymentRefundModule.Application.Exceptions;
using PaymentRefundModule.Domain.Rules;

namespace PaymentRefundModule.Application.UseCases.ReplacePayment;

internal sealed class ReplacePayment_UCInput_Validator : AbstractValidator<ReplacePayment_UCInput>
{
    public ReplacePayment_UCInput_Validator()
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
