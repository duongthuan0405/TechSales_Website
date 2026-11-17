using AuthModule.Application.Exceptions;
using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.Register;
internal sealed class Register_UCInput_Validator : AbstractValidator<Register_UCInput>
{
    public Register_UCInput_Validator()
    {
        RuleFor(x => x.Email).Must(AuthRules.ValidEmail).WithMessage(BusinessExceptionMessage.EmailInputInvalid);
        RuleFor(x => x.Password).Must(AuthRules.ValidPassword).WithMessage(AuthRules.PasswordRequirementMessage);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage(BusinessExceptionMessage.PasswordConfirmationMismatch);
    }
}
