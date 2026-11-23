using FluentValidation;
using PaymentRefundModule.Application.Exceptions;

namespace PaymentRefundModule.Application.UseCases.ReceiveVnpayIpn;

internal sealed class ReceiveVnpayIpn_UCInput_Validator : AbstractValidator<ReceiveVnpayIpn_UCInput>
{
    public ReceiveVnpayIpn_UCInput_Validator()
    {
        RuleFor(input => input.Parameters).NotNull().WithMessage(BusinessExceptionMessage.PaymentNotificationInvalid);
    }
}
