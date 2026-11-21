using CartModule.Application.Exceptions;
using CartModule.Domain.Rules;
using FluentValidation;

namespace CartModule.Application.UseCases.Carts.RemoveCartItem;

internal sealed class RemoveCartItem_UCInput_Validator : AbstractValidator<RemoveCartItem_UCInput>
{
    public RemoveCartItem_UCInput_Validator()
    {
        RuleFor(input => input.UserId)
            .Must(CartRules.IsValidId)
            .WithMessage(BusinessExceptionMessage.InvalidUserId);

        RuleFor(input => input.ProductId)
            .Must(CartRules.IsValidId)
            .WithMessage(BusinessExceptionMessage.InvalidProductId);
    }
}
