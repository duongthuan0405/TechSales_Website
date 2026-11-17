using FluentValidation;

namespace PromotionModule.Application.UseCases.GetVoucher;

internal sealed class GetVoucher_UCInput_Validator : AbstractValidator<GetVoucher_UCInput>
{
    public GetVoucher_UCInput_Validator()
    {
        RuleFor(input => input.Id).NotEmpty();
    }
}
