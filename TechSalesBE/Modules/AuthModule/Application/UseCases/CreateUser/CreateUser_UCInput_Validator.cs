using AuthModule.Domain.Constants;
using AuthModule.Application.Constants;
using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.CreateUser;
internal sealed class CreateUser_UCInput_Validator : AbstractValidator<CreateUser_UCInput>
{
    public CreateUser_UCInput_Validator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.Email).Must(AuthRules.ValidEmail).WithMessage("Invalid email address.");
        RuleFor(x => x.Password).Must(AuthRules.ValidPassword).WithMessage(AuthRules.PasswordRequirementMessage);
        RuleFor(x => x.Roles).NotNull().Must(r => r is { Length: > 0 and <= AuthLimits.MaxRolesPerUser } && r.Distinct().Count() == r.Length).WithMessage("Choose distinct valid roles.");
        RuleForEach(x => x.Roles).Must(r => r is UserRoleName.CUSTOMER or UserRoleName.STAFF or UserRoleName.BUSINESS_ADMIN or UserRoleName.TECHNICAL_ADMIN);
    }
}
