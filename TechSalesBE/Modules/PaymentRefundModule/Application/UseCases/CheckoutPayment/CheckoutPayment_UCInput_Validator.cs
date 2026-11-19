using FluentValidation;
using PaymentRefundModule.Application.Exceptions;

namespace PaymentRefundModule.Application.UseCases.CheckoutPayment;

internal sealed class CheckoutPayment_UCInput_Validator : AbstractValidator<CheckoutPayment_UCInput>
{
    public CheckoutPayment_UCInput_Validator()
    {
        RuleFor(request => request.CustomerId).NotEmpty().WithMessage(BusinessExceptionMessage.CustomerIdInvalid);
        RuleFor(request => request.PaymentId).NotEmpty().WithMessage(BusinessExceptionMessage.PaymentIdInvalid);
    }
}
