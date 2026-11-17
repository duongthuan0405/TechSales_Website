namespace AuthModule.Application.UseCases.UpdateUser;

internal sealed class UpdateUser_UCOutput
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LockedUntil { get; init; }
    public List<string> Roles { get; init; } = new List<string>();
    public ProfileData Profile { get; init; } = new ProfileData();

    public sealed class ProfileData
    {
        public string FullName { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string? AvatarUrl { get; init; }
        public DateOnly? DateOfBirth { get; init; }
    }
}
