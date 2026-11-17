namespace AuthModule.Presentation.Authorization;

internal sealed class AccountManagementResource
{
    public Guid? TargetUserId { get; init; }
    public string[]? RequestedRoles { get; init; }
}
