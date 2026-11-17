using AuthModule.Domain.Rules;
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

        UserProfileRules.EnsureValid(name, number, avatarUrl, birthDate, today);

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
