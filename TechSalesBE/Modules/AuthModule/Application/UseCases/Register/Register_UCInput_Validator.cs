using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.Register;
internal sealed class Register_UCInput_Validator : AbstractValidator<Register_UCInput>
{
    public Register_UCInput_Validator()
    {
        RuleFor(x => x.Email).Must(AuthRules.ValidEmail).WithMessage("Invalid email address.");
        RuleFor(x => x.Password).Must(AuthRules.ValidPassword).WithMessage(AuthRules.PasswordRequirementMessage);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match.");
    }
}
