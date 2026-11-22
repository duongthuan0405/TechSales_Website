using CatalogModule.Application.Exceptions;
using CatalogModule.Domain.Exceptions;
using CatalogModule.Domain.Rules;
using FluentValidation;

namespace CatalogModule.Application.UseCases.UpdateBrand;

internal sealed class UpdateBrand_UCInput_Validator : AbstractValidator<UpdateBrand_UCInput>
{
    public UpdateBrand_UCInput_Validator()
    {
        RuleFor(input => input.Id)
            .NotEmpty()
            .WithMessage(BusinessExceptionMessage.InvalidBrandId);
        RuleFor(input => input.Name)
            .Must(BrandRules.ValidName)
            .WithMessage(DomainExceptionMessage.InvalidBrand);
    }
}
