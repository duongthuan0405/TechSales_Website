using FluentValidation;
using InventoryModule.Application.Exceptions;
using InventoryModule.Domain.Rules;

namespace InventoryModule.Application.UseCases.BrowseProductUnits;

internal sealed class BrowseProductUnits_UCInput_Validator : AbstractValidator<BrowseProductUnits_UCInput>
{
    public BrowseProductUnits_UCInput_Validator()
    {
        RuleFor(input => input.PageNumber).GreaterThan(0).WithMessage(BusinessExceptionMessage.InvalidUnitPage);
        RuleFor(input => input.PageSize).InclusiveBetween(1, 100).WithMessage(BusinessExceptionMessage.InvalidUnitPage);
        RuleFor(input => input.ProductId).Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage(BusinessExceptionMessage.InvalidProductId);
        RuleFor(input => input.Status).Must(ProductUnitRules.IsBrowseStatus)
            .WithMessage(BusinessExceptionMessage.InvalidUnitFilter);
    }
}
