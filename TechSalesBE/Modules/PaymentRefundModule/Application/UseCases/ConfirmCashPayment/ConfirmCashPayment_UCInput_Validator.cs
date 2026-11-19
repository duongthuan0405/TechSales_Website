using FluentValidation;
using PaymentRefundModule.Application.Exceptions;

namespace PaymentRefundModule.Application.UseCases.ConfirmCashPayment;

internal sealed class ConfirmCashPayment_UCInput_Validator : AbstractValidator<ConfirmCashPayment_UCInput>
{
    public ConfirmCashPayment_UCInput_Validator()
    {
        RuleFor(request => request.Id).NotEmpty().WithMessage(BusinessExceptionMessage.PaymentIdInvalid);
    }
}
