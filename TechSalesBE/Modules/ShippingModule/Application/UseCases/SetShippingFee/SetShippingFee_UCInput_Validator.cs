using ShippingModule.Application.Exceptions;
using FluentValidation;
using ShippingModule.Domain.Rules;

namespace ShippingModule.Application.UseCases.SetShippingFee;

internal sealed class SetShippingFee_UCInput_Validator : AbstractValidator<SetShippingFee_UCInput>
{
    public SetShippingFee_UCInput_Validator()
    {
        RuleFor(input => input.ProvinceCode).Must(ShippingRules.ValidAdministrativeCode)
            .WithMessage(BusinessExceptionMessage.ProvinceCodeInvalid);
        RuleFor(input => input.Fee).Must(ShippingRules.ValidShippingFee)
            .WithMessage(BusinessExceptionMessage.NegativeShippingFee);
    }
}
