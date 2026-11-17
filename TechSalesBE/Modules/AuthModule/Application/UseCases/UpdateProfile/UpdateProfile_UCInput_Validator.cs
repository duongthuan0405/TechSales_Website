using AuthModule.Domain.Constants;
using AuthModule.Domain.Rules;
using FluentValidation;
namespace AuthModule.Application.UseCases.UpdateProfile;
internal sealed class UpdateProfile_UCInput_Validator : AbstractValidator<UpdateProfile_UCInput>
{
    public UpdateProfile_UCInput_Validator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.FullName)
            .Must(AuthRules.ValidFullName)
            .When(input => input.FullName != null)
            .WithMessage("Invalid full name.");

        RuleFor(x => x.Phone)
            .Must(AuthRules.ValidPhone)
            .When(input => input.Phone != null)
            .WithMessage(AuthRules.PhoneRequirementMessage);

        RuleFor(x => x.AvatarUrl)
            .Must(AuthRules.ValidAvatarUrl)
            .WithMessage("Invalid avatar URL.");

        RuleFor(x => x.DateOfBirth)
            .Must(dateOfBirth => AuthRules.ValidDateOfBirth(dateOfBirth, DateOnly.FromDateTime(DateTime.UtcNow)))
            .WithMessage("Date of birth cannot be in the future.");
        RuleFor(x => x.AvatarLength).InclusiveBetween(0, AuthLimits.AvatarMaxSizeBytes);
    }
}
