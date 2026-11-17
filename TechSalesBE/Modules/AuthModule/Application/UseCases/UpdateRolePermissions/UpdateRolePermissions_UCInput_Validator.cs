using AuthModule.Application.Exceptions;
using AuthModule.Domain.Constants;
using AuthModule.Application.Constants;
using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.UpdateRolePermissions;
internal sealed class UpdateRolePermissions_UCInput_Validator : AbstractValidator<UpdateRolePermissions_UCInput>
{
    public UpdateRolePermissions_UCInput_Validator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.PermissionIds).NotNull().Must(x => x is { Length: <= AuthLimits.MaxPermissionsPerRole } && x.Distinct().Count() == x.Length).WithMessage(BusinessExceptionMessage.PermissionIdsMustBeDistinct);
        RuleForEach(x => x.PermissionIds).NotEmpty();
    }
}
