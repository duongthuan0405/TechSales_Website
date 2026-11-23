using CatalogModule.Application.Constants;
using CatalogModule.Application.Exceptions;
using FluentValidation;

namespace CatalogModule.Application.UseCases.BrowseCategoriesForManagement;

internal sealed class BrowseCategoriesForManagement_UCInput_Validator : AbstractValidator<BrowseCategoriesForManagement_UCInput>
{
    public BrowseCategoriesForManagement_UCInput_Validator()
    {
        RuleFor(input => input.PageNumber).InclusiveBetween(1, CatalogPagination.MaximumPageNumber)
            .WithMessage(BusinessExceptionMessage.PaginationInvalid);
        RuleFor(input => input.PageSize).InclusiveBetween(1, CatalogPagination.MaximumPageSize)
            .WithMessage(BusinessExceptionMessage.PaginationInvalid);
    }
}
