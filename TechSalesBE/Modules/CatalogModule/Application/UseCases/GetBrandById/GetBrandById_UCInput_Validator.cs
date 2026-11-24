using CatalogModule.Application.Exceptions;
using FluentValidation;

namespace CatalogModule.Application.UseCases.GetBrandById;

internal sealed class GetBrandById_UCInput_Validator : AbstractValidator<GetBrandById_UCInput>
{
    public GetBrandById_UCInput_Validator()
    {
        RuleFor(input => input.Id)
            .NotEmpty()
            .WithMessage(BusinessExceptionMessage.InvalidBrandId);
    }
}
