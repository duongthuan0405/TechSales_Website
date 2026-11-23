using CatalogModule.Application.Constants;
using CatalogModule.Application.Exceptions;
using CatalogModule.Domain.Rules;
using FluentValidation;

namespace CatalogModule.Application.UseCases.BrowseProducts;

internal sealed class BrowseProducts_UCInput_Validator : AbstractValidator<BrowseProducts_UCInput>
{
    public BrowseProducts_UCInput_Validator()
    {
        RuleFor(input => input.PageNumber).InclusiveBetween(1, ProductPagination.MaximumPageNumber)
            .WithMessage(BusinessExceptionMessage.PaginationInvalid);
        RuleFor(input => input.PageSize).InclusiveBetween(1, ProductPagination.MaximumPageSize)
            .WithMessage(BusinessExceptionMessage.PaginationInvalid);
        RuleFor(input => input.Status).Must(status => status == null || ProductRules.ValidStatus(status))
            .WithMessage(BusinessExceptionMessage.ProductStatusInvalid);
    }
}
