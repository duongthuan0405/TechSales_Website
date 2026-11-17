using FluentValidation;
using PromotionModule.Application.Exceptions;
using PromotionModule.Application.Constants;

namespace PromotionModule.Application.UseCases.GetVouchers;

internal sealed class GetVouchers_UCInput_Validator : AbstractValidator<GetVouchers_UCInput>
{
    public GetVouchers_UCInput_Validator()
    {
        RuleFor(input => input.PageNumber).InclusiveBetween(PaginationLimits.MinimumPageNumber, PaginationLimits.MaximumPageNumber)
            .WithMessage(BusinessExceptionMessage.InvalidPage);
        RuleFor(input => input.PageSize).InclusiveBetween(PaginationLimits.MinimumPageSize, PaginationLimits.MaximumPageSize)
            .WithMessage(BusinessExceptionMessage.InvalidPage);
    }
}
