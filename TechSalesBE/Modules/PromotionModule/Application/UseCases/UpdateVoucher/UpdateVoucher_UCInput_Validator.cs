using FluentValidation;
using PromotionModule.Application.Exceptions;
using PromotionModule.Domain.Rules;

namespace PromotionModule.Application.UseCases.UpdateVoucher;

internal sealed class UpdateVoucher_UCInput_Validator : AbstractValidator<UpdateVoucher_UCInput>
{
    public UpdateVoucher_UCInput_Validator()
    {
        RuleFor(input => input.Id).NotEmpty();
        RuleFor(input => input.Code).Must(VoucherRules.ValidCode).WithMessage(BusinessExceptionMessage.InvalidCode);
        RuleFor(input => input.Type).Must(VoucherRules.ValidType).WithMessage(BusinessExceptionMessage.InvalidType);
        RuleFor(input => input.Value).Must((input, value) => VoucherRules.ValidValue(input.Type, value))
            .WithMessage(BusinessExceptionMessage.InvalidValue);
        RuleFor(input => input.MaxUsage).Must(VoucherRules.ValidMaximumUsage)
            .WithMessage(BusinessExceptionMessage.InvalidMaximumUsage);
        RuleFor(input => input.MinOrderAmount).Must(VoucherRules.ValidMinimumOrder)
            .WithMessage(BusinessExceptionMessage.InvalidMinimumOrder);
        RuleFor(input => input.EndDate).Must((input, endDate) => VoucherRules.ValidDateRange(input.StartDate, endDate))
            .WithMessage(BusinessExceptionMessage.InvalidDateRange);
    }
}
