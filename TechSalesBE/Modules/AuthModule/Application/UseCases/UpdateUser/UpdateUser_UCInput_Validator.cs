using AuthModule.Application.Exceptions;
using AuthModule.Application.Constants;
using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.UpdateUser;
internal sealed class UpdateUser_UCInput_Validator : AbstractValidator<UpdateUser_UCInput>
{
    public UpdateUser_UCInput_Validator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Email).Must(AuthRules.ValidEmail).WithMessage(BusinessExceptionMessage.EmailInputInvalid);
    }
}
