using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.VerifyEmail;
internal sealed class VerifyEmail_UCInput_Validator : AbstractValidator<VerifyEmail_UCInput>
{
    public VerifyEmail_UCInput_Validator()
    {
        RuleFor(x => x.Email).Must(AuthRules.ValidEmail).WithMessage("Invalid email address.");
        RuleFor(x => x.Token).NotEmpty().Matches(AuthRules.OtpPattern);
    }
}
