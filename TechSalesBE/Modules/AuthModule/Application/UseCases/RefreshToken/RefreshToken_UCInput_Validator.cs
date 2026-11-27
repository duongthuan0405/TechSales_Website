using FluentValidation;
namespace AuthModule.Application.UseCases.RefreshToken;
internal sealed class RefreshToken_UCInput_Validator : AbstractValidator<RefreshToken_UCInput>
{
    public RefreshToken_UCInput_Validator() { RuleFor(x => x.RefreshToken).MaximumLength(256); }
}
