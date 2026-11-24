using FluentValidation;
using AuthModule.Application.Exceptions;

namespace AuthModule.Application.UseCases.DeactivateUser;

internal sealed class DeactivateUser_UCInput_Validator : AbstractValidator<DeactivateUser_UCInput>
{
    public DeactivateUser_UCInput_Validator()
    {
        RuleFor(input => input.ActorId).NotEmpty().WithMessage(BusinessExceptionMessage.ActorNotFound);
        RuleFor(input => input.UserId).NotEmpty().WithMessage(BusinessExceptionMessage.UserNotFound);
    }
}
