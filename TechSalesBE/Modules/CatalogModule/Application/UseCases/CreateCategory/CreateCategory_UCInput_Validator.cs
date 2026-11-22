using CatalogModule.Domain.Exceptions;
using CatalogModule.Domain.Rules;
using FluentValidation;

namespace CatalogModule.Application.UseCases.CreateCategory;

internal sealed class CreateCategory_UCInput_Validator : AbstractValidator<CreateCategory_UCInput>
{
    public CreateCategory_UCInput_Validator()
    {
        RuleFor(input => input.Name)
            .Must(CategoryRules.ValidName)
            .WithMessage(DomainExceptionMessage.InvalidCategory);
    }
}
