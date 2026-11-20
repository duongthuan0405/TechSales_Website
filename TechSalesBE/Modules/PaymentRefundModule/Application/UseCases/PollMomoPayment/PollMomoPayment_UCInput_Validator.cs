using FluentValidation;
using PaymentRefundModule.Application.Exceptions;
namespace PaymentRefundModule.Application.UseCases.PollMomoPayment;
internal sealed class PollMomoPayment_UCInput_Validator : AbstractValidator<PollMomoPayment_UCInput>
{
    public PollMomoPayment_UCInput_Validator()
    {
        RuleFor(input => input.PaymentId).NotEmpty().WithMessage(BusinessExceptionMessage.PaymentIdInvalid);
    }
}

