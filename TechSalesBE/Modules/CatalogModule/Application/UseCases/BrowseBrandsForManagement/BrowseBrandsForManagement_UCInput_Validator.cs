using CatalogModule.Application.Constants;
using CatalogModule.Application.Exceptions;
using FluentValidation;

namespace CatalogModule.Application.UseCases.BrowseBrandsForManagement;

internal sealed class BrowseBrandsForManagement_UCInput_Validator : AbstractValidator<BrowseBrandsForManagement_UCInput>
{
    public BrowseBrandsForManagement_UCInput_Validator()
    {
        RuleFor(input => input.PageNumber).InclusiveBetween(1, CatalogPagination.MaximumPageNumber)
            .WithMessage(BusinessExceptionMessage.PaginationInvalid);
        RuleFor(input => input.PageSize).InclusiveBetween(1, CatalogPagination.MaximumPageSize)
            .WithMessage(BusinessExceptionMessage.PaginationInvalid);
    }
}
