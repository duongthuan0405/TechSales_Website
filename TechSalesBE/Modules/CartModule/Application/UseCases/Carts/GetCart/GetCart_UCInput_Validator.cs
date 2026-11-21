using CartModule.Application.Exceptions;
using CartModule.Domain.Rules;
using FluentValidation;

namespace CartModule.Application.UseCases.Carts.GetCart;

internal sealed class GetCart_UCInput_Validator : AbstractValidator<GetCart_UCInput>
{
    public GetCart_UCInput_Validator()
    {
        RuleFor(input => input.UserId)
            .Must(CartRules.IsValidId)
            .WithMessage(BusinessExceptionMessage.InvalidUserId);
    }
}
