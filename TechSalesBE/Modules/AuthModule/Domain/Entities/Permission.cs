namespace AuthModule.Domain.Entities;
internal sealed class Permission
{
    public Guid Id { get; init; }
    public string Code { get; init; }
    public string Name { get; init; }
    public string Module { get; init; }

    public Permission(Guid id, string code, string name, string module)
    {
        Id = id;
        Code = code;
        Name = name;
        Module = module;
    }
}
