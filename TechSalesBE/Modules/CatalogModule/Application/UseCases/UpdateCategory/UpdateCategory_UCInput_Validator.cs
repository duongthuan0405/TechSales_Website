using CatalogModule.Application.Exceptions;
using CatalogModule.Domain.Exceptions;
using CatalogModule.Domain.Rules;
using FluentValidation;

namespace CatalogModule.Application.UseCases.UpdateCategory;

internal sealed class UpdateCategory_UCInput_Validator : AbstractValidator<UpdateCategory_UCInput>
{
    public UpdateCategory_UCInput_Validator()
    {
        RuleFor(input => input.Id)
            .NotEmpty()
            .WithMessage(BusinessExceptionMessage.InvalidCategoryId);
        RuleFor(input => input.Name)
            .Must(CategoryRules.ValidName)
            .WithMessage(DomainExceptionMessage.InvalidCategory);
    }
}
