using AuthModule.Domain.Exceptions;
using AuthModule.Domain.Rules;
using CommonModule.Domain.Exceptions;
namespace AuthModule.Domain.Entities;
internal sealed class UserProfile
{
    internal string FullName { get; set; } = string.Empty;
    internal string PhoneNumber { get; set; } = string.Empty;
    internal string? AvatarUrl { get; set; }
    internal DateOnly? DateOfBirth { get; set; }
    internal void Update(string? fullName, string? phone, string? avatarUrl, DateOnly? birthDate, DateOnly today)
    {
        string name = fullName ?? FullName;
        string number = phone ?? PhoneNumber;

        if (!AuthRules.ValidFullName(name))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PROFILE, "Invalid full name.");
        }

        if (!AuthRules.ValidPhone(number))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PROFILE, "Invalid phone number.");
        }

        if (!AuthRules.ValidAvatarUrl(avatarUrl))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PROFILE, "Invalid avatar URL.");
        }

        if (!AuthRules.ValidDateOfBirth(birthDate, today))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PROFILE, "Invalid date of birth.");
        }

        FullName = name.Trim();
        PhoneNumber = number;

        if (avatarUrl != null)
        {
            AvatarUrl = avatarUrl;
        }

        if (birthDate.HasValue)
        {
            DateOfBirth = birthDate.Value;
        }
    }
}
