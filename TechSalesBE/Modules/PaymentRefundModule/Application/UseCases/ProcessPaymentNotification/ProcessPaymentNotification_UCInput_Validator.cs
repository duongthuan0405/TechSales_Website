using FluentValidation;
using PaymentRefundModule.Application.Exceptions;

namespace PaymentRefundModule.Application.UseCases.ProcessPaymentNotification;

internal sealed class ProcessPaymentNotification_UCInput_Validator : AbstractValidator<ProcessPaymentNotification_UCInput>
{
    public ProcessPaymentNotification_UCInput_Validator()
    {
        RuleFor(input => input.PaymentId).NotEmpty().WithMessage(BusinessExceptionMessage.PaymentNotificationInvalid);
        RuleFor(input => input.Amount).GreaterThan(0).WithMessage(BusinessExceptionMessage.PaymentNotificationInvalid);
        RuleFor(input => input.GatewayCode).NotEmpty().WithMessage(BusinessExceptionMessage.PaymentNotificationInvalid);
        RuleFor(input => input.TransactionRef).NotEmpty().WithMessage(BusinessExceptionMessage.PaymentNotificationInvalid);
        RuleFor(input => input.Status).IsInEnum().WithMessage(BusinessExceptionMessage.PaymentNotificationInvalid);
    }
}
