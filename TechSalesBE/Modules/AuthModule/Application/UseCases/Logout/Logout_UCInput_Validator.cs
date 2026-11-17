using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.Logout;
internal sealed class Logout_UCInput_Validator : AbstractValidator<Logout_UCInput>
{
    public Logout_UCInput_Validator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
    }
}
