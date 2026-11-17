using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.ForgotPassword;
internal sealed class ForgotPassword_UCInput_Validator : AbstractValidator<ForgotPassword_UCInput>
{
    public ForgotPassword_UCInput_Validator()
    {
        RuleFor(x => x.Email).Must(AuthRules.ValidEmail).WithMessage("Invalid email address.");
    }
}
