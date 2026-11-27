using CartModule.Application.Exceptions;
using CartModule.Domain.Rules;
using FluentValidation;

namespace CartModule.Application.UseCases.Carts.UpdateCartItem;

internal sealed class UpdateCartItem_UCInput_Validator : AbstractValidator<UpdateCartItem_UCInput>
{
    public UpdateCartItem_UCInput_Validator()
    {
        RuleFor(input => input.UserId)
            .Must(CartRules.IsValidId)
            .WithMessage(BusinessExceptionMessage.InvalidUserId);

        RuleFor(input => input.ProductId)
            .Must(CartRules.IsValidId)
            .WithMessage(BusinessExceptionMessage.InvalidProductId);

        RuleFor(input => input.Quantity)
            .Must(CartRules.IsPositiveQuantity)
            .WithMessage(BusinessExceptionMessage.InvalidUpdatedQuantity);
    }
}
