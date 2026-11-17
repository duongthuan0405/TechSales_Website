using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.ResetPassword;
internal sealed class ResetPassword_UCInput_Validator : AbstractValidator<ResetPassword_UCInput>
{
    public ResetPassword_UCInput_Validator()
    {
        RuleFor(x => x.Email).Must(AuthRules.ValidEmail).WithMessage("Invalid email address.");
        RuleFor(x => x.Token).NotEmpty().Matches(AuthRules.OtpPattern);
        RuleFor(x => x.NewPassword).Must(AuthRules.ValidPassword).WithMessage(AuthRules.PasswordRequirementMessage);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword);
    }
}
