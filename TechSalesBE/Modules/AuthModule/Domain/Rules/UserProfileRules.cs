using AuthModule.Domain.Exceptions;
using CommonModule.Domain.Exceptions;

namespace AuthModule.Domain.Rules;

internal static class UserProfileRules
{
    internal static void EnsureValid(string name, string number, string? avatarUrl, DateOnly? birthDate, DateOnly today)
    {
        if (!AuthRules.ValidFullName(name))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PROFILE, DomainExceptionMessage.FullNameInvalid);
        }

        if (!AuthRules.ValidPhone(number))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PROFILE, DomainExceptionMessage.PhoneInvalid);
        }

        if (!AuthRules.ValidAvatarUrl(avatarUrl))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PROFILE, DomainExceptionMessage.AvatarInvalid);
        }

        if (!AuthRules.ValidDateOfBirth(birthDate, today))
        {
            throw new DomainException(DomainExceptionCode.INVALID_PROFILE, DomainExceptionMessage.BirthDateInvalid);
        }

    }
}
