using FluentValidation;

namespace PromotionModule.Application.UseCases.DeactivateVoucher;

internal sealed class DeactivateVoucher_UCInput_Validator : AbstractValidator<DeactivateVoucher_UCInput>
{
    public DeactivateVoucher_UCInput_Validator()
    {
        RuleFor(input => input.Id).NotEmpty();
    }
}
