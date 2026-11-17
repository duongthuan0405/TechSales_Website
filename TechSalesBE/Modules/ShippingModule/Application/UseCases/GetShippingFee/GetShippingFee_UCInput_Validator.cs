using ShippingModule.Application.Exceptions;
using FluentValidation;
using ShippingModule.Domain.Rules;

namespace ShippingModule.Application.UseCases.GetShippingFee;

internal sealed class GetShippingFee_UCInput_Validator : AbstractValidator<GetShippingFee_UCInput>
{
    public GetShippingFee_UCInput_Validator()
    {
        RuleFor(input => input).Must(HasExactlyOneLocation)
            .WithMessage(BusinessExceptionMessage.LocationSelectionInvalid);
        RuleFor(input => input.ProvinceCode).Must(ShippingRules.ValidAdministrativeCode)
            .When(input => input.ProvinceCode != null).WithMessage(BusinessExceptionMessage.ProvinceCodeInvalid);
        RuleFor(input => input.WardCode).Must(ShippingRules.ValidAdministrativeCode)
            .When(input => input.WardCode != null).WithMessage(BusinessExceptionMessage.WardCodeInvalid);
    }

    private static bool HasExactlyOneLocation(GetShippingFee_UCInput input)
    {
        bool hasProvince = input.ProvinceCode != null;
        bool hasWard = input.WardCode != null;
        return hasProvince != hasWard;
    }
}
