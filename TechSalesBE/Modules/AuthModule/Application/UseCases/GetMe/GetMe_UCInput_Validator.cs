using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.GetMe;
internal sealed class GetMe_UCInput_Validator : AbstractValidator<GetMe_UCInput>
{
    public GetMe_UCInput_Validator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
    }
}
