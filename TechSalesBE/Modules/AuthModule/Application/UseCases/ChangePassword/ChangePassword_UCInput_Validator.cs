using AuthModule.Domain.Constants;
using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.ChangePassword;
internal sealed class ChangePassword_UCInput_Validator : AbstractValidator<ChangePassword_UCInput>
{
    public ChangePassword_UCInput_Validator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.CurrentPassword).NotEmpty().MaximumLength(AuthLimits.PasswordInputMaxLength);
        RuleFor(x => x.NewPassword).Must(AuthRules.ValidPassword).WithMessage(AuthRules.PasswordRequirementMessage);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword);
    }
}
