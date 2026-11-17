using AuthModule.Application.Constants;
using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.UnlockUser;
internal sealed class UnlockUser_UCInput_Validator : AbstractValidator<UnlockUser_UCInput>
{
    public UnlockUser_UCInput_Validator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
