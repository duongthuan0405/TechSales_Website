namespace AuthModule.Application.UseCases.GetPermissions;

internal sealed class GetPermissions_UCOutput
{
    public List<PermissionItem> Permissions { get; init; } = new List<PermissionItem>();

    public sealed class PermissionItem
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Module { get; init; } = string.Empty;
    }
}
