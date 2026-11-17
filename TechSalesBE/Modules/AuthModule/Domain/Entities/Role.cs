namespace AuthModule.Domain.Entities;
internal sealed class Role
{
    public List<Permission> Permissions { get; } = new List<Permission>();

    public Guid Id { get; init; }
    public string Name { get; init; }
    public string? Description { get; init; }

    public Role(Guid id, string name, string? description = null)
    {
        Id = id;
        Name = name;
        Description = description;
    }
}
