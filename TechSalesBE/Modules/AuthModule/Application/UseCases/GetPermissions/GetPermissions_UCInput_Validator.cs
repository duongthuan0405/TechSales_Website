using AuthModule.Application.Constants;
using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.GetPermissions;
internal sealed class GetPermissions_UCInput_Validator : AbstractValidator<GetPermissions_UCInput>
{
    public GetPermissions_UCInput_Validator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
    }
}
