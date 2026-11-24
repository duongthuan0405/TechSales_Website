using CatalogModule.Domain.Exceptions;
using CatalogModule.Domain.Rules;
using FluentValidation;

namespace CatalogModule.Application.UseCases.CreateBrand;

internal sealed class CreateBrand_UCInput_Validator : AbstractValidator<CreateBrand_UCInput>
{
    public CreateBrand_UCInput_Validator()
    {
        RuleFor(input => input.Name)
            .Must(BrandRules.ValidName)
            .WithMessage(DomainExceptionMessage.InvalidBrand);
    }
}
