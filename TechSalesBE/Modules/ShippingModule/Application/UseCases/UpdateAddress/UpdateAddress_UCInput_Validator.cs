using ShippingModule.Application.Exceptions;
using FluentValidation;
using ShippingModule.Domain.Rules;
using ShippingModule.Domain.Constants;

namespace ShippingModule.Application.UseCases.UpdateAddress;

internal sealed class UpdateAddress_UCInput_Validator : AbstractValidator<UpdateAddress_UCInput>
{
    public UpdateAddress_UCInput_Validator()
    {
        RuleFor(input => input.ActorId).NotEmpty();
        RuleFor(input => input.AddressId).NotEmpty();
        RuleFor(input => input.WardCode).Must(ShippingRules.ValidAdministrativeCode)
            .WithMessage(BusinessExceptionMessage.WardCodeInvalid);
        RuleFor(input => input.Detail).Must(ShippingRules.ValidAddressDetail)
            .WithMessage(BusinessExceptionMessage.AddressDetailInvalid);
    }
}
