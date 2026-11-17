using AuthModule.Application.Constants;
using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.LockUser;
internal sealed class LockUser_UCInput_Validator : AbstractValidator<LockUser_UCInput>
{
    public LockUser_UCInput_Validator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Until).Must(x => x is null || x > DateTimeOffset.UtcNow).WithMessage("Lock expiry must be in the future.");
    }
}
