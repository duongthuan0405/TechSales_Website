namespace AuthModule.Application.UseCases.GetRoles;

internal sealed class GetRoles_UCOutput
{
    public List<RoleItem> Roles { get; init; } = new List<RoleItem>();

    public sealed class RoleItem
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public List<PermissionItem> Permissions { get; init; } = new List<PermissionItem>();
    }

    public sealed class PermissionItem
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Module { get; init; } = string.Empty;
    }
}
