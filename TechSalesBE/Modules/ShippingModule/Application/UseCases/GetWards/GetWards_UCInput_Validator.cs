using ShippingModule.Application.Exceptions;
using FluentValidation;
using ShippingModule.Domain.Rules;

namespace ShippingModule.Application.UseCases.GetWards;

internal sealed class GetWards_UCInput_Validator : AbstractValidator<GetWards_UCInput>
{
    public GetWards_UCInput_Validator()
    {
        RuleFor(input => input.ProvinceCode).Must(ShippingRules.ValidAdministrativeCode)
            .WithMessage(BusinessExceptionMessage.ProvinceCodeInvalid);
    }
}
