using AuthModule.Application.Exceptions;
using AuthModule.Domain.Constants;
using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.Login;
internal sealed class Login_UCInput_Validator : AbstractValidator<Login_UCInput>
{
    public Login_UCInput_Validator()
    {
        RuleFor(x => x.Email).Must(AuthRules.ValidEmail).WithMessage(BusinessExceptionMessage.EmailInputInvalid);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(AuthLimits.PasswordInputMaxLength);
    }
}
