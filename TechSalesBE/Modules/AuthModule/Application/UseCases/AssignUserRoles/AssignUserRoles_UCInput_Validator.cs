using AuthModule.Domain.Constants;
using AuthModule.Application.Constants;
using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.AssignUserRoles;
internal sealed class AssignUserRoles_UCInput_Validator : AbstractValidator<AssignUserRoles_UCInput>
{
    public AssignUserRoles_UCInput_Validator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleIds).NotNull().Must(x => x is { Length: > 0 and <= AuthLimits.MaxRolesPerUser } && x.Distinct().Count() == x.Length).WithMessage("Choose distinct roles.");
        RuleForEach(x => x.RoleIds).NotEmpty();
    }
}
