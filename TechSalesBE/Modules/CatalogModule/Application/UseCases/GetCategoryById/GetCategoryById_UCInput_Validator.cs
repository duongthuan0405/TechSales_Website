using CatalogModule.Application.Exceptions;
using FluentValidation;

namespace CatalogModule.Application.UseCases.GetCategoryById;

internal sealed class GetCategoryById_UCInput_Validator : AbstractValidator<GetCategoryById_UCInput>
{
    public GetCategoryById_UCInput_Validator()
    {
        RuleFor(input => input.Id)
            .NotEmpty()
            .WithMessage(BusinessExceptionMessage.InvalidCategoryId);
    }
}
