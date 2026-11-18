using FluentValidation;
using PromotionModule.Application.Constants;
using PromotionModule.Application.Exceptions;

namespace PromotionModule.Application.UseCases.GetVouchersForCustomer;

internal sealed class GetVouchersForCustomer_UCInput_Validator : AbstractValidator<GetVouchersForCustomer_UCInput>
{
    public GetVouchersForCustomer_UCInput_Validator()
    {
        RuleFor(input => input.PageNumber).InclusiveBetween(PaginationLimits.MinimumPageNumber, PaginationLimits.MaximumPageNumber)
            .WithMessage(BusinessExceptionMessage.InvalidPage);
        RuleFor(input => input.PageSize).InclusiveBetween(PaginationLimits.MinimumPageSize, PaginationLimits.MaximumPageSize)
            .WithMessage(BusinessExceptionMessage.InvalidPage);
    }
}
