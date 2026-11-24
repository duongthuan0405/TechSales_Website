using AuthModule.Application.Exceptions;
using AuthModule.Domain.Constants;
using AuthModule.Application.Constants;
using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.CreateUser;
internal sealed class CreateUser_UCInput_Validator : AbstractValidator<CreateUser_UCInput>
{
    public CreateUser_UCInput_Validator()
    {
        RuleFor(x => x.FullName).Must(AuthRules.ValidFullName).WithMessage(BusinessExceptionMessage.FullNameInvalid);
        RuleFor(x => x.Phone).Must(AuthRules.ValidPhone).WithMessage("Số điện thoại phải gồm 10–11 chữ số.");
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.Email).Must(AuthRules.ValidEmail).WithMessage(BusinessExceptionMessage.EmailInputInvalid);
        RuleFor(x => x.Password).Must(AuthRules.ValidPassword).WithMessage(AuthRules.PasswordRequirementMessage);
        RuleFor(x => x.Roles).NotNull().Must(r => r is { Length: > 0 and <= AuthLimits.MaxRolesPerUser } && r.Distinct().Count() == r.Length).WithMessage(BusinessExceptionMessage.RolesInvalid);
        RuleForEach(x => x.Roles).Must(r => r is UserRoleName.CUSTOMER or UserRoleName.STAFF or UserRoleName.BUSINESS_ADMIN or UserRoleName.TECHNICAL_ADMIN);
    }
}
