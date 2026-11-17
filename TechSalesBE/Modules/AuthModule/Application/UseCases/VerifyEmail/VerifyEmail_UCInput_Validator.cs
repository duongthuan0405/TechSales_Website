using AuthModule.Application.Exceptions;
using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.VerifyEmail;
internal sealed class VerifyEmail_UCInput_Validator : AbstractValidator<VerifyEmail_UCInput>
{
    public VerifyEmail_UCInput_Validator()
    {
        RuleFor(x => x.Email).Must(AuthRules.ValidEmail).WithMessage(BusinessExceptionMessage.EmailInputInvalid);
        RuleFor(x => x.Token).NotEmpty().Matches(AuthRules.OtpPattern);
    }
}
