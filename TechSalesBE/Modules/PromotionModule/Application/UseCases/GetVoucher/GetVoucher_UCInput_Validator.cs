using FluentValidation;

namespace PromotionModule.Application.UseCases.GetVoucher;

internal sealed class GetVoucherForCustomer_UCInput_Validator : AbstractValidator<GetVoucherForCustomer_UCInput>
{
    public GetVoucherForCustomer_UCInput_Validator()
    {
        RuleFor(input => input.Id).NotEmpty();
    }
}
