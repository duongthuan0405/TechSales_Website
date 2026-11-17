using AuthModule.Application.Constants;
using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.GetRoles;
internal sealed class GetRoles_UCInput_Validator : AbstractValidator<GetRoles_UCInput>
{
    public GetRoles_UCInput_Validator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
    }
}
