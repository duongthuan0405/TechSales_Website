using FluentValidation;
using PaymentRefundModule.Application.Exceptions;

namespace PaymentRefundModule.Application.UseCases.ReceiveMomoIpn;

internal sealed class ReceiveMomoIpn_UCInput_Validator : AbstractValidator<ReceiveMomoIpn_UCInput>
{
    public ReceiveMomoIpn_UCInput_Validator()
    {
        RuleFor(input => input.OrderId)
            .Must(value => Guid.TryParseExact(value, "N", out Guid id) && id != Guid.Empty)
            .WithMessage(BusinessExceptionMessage.MomoIpnInvalid);
        RuleFor(input => input.Amount).GreaterThan(0).WithMessage(BusinessExceptionMessage.MomoIpnInvalid);
        RuleFor(input => input.RequestId).NotEmpty().WithMessage(BusinessExceptionMessage.MomoIpnInvalid);
        RuleFor(input => input.TransId).GreaterThan(0).WithMessage(BusinessExceptionMessage.MomoIpnInvalid);
    }
}
